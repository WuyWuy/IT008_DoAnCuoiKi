using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using QuanLyCaPhe.DAO; // added
using System.Linq; // added
using QuanLyCaPhe.Models;
using System.Windows.Threading;

namespace QuanLyCaPhe.Views.SharedPage
{
    public partial class SchedulePage : Page
    {
        // Cấu hình thời gian
        private const int StartHour = 6;
        private const int EndHour = 22;
        private const int StepMinutes = 30;

        // per-user color mapping
        private Dictionary<int, Brush> _userBrushes = new Dictionary<int, Brush>();
        private List<Brush> _palette = new List<Brush>
        {
            Brushes.Red,
            Brushes.Blue,
            Brushes.Green,
            Brushes.Yellow,
            Brushes.Pink,
            Brushes.Gray,
            Brushes.Aqua,
            Brushes.Orange,
            Brushes.Purple,
            Brushes.White
        };
        private int _nextPaletteIndex = 0;

        // Quản lý thời gian hiện tại
        private DateTime _currentMonthDisplay;
        private DateTime _selectedDate; // Ngày được chọn khi click
        private TimeSpan _selectedStartTime; // Giờ bắt đầu khi click

        // Biến lưu trữ danh sách các ca đã thêm để kiểm tra va chạm
        // Key: "DayIndex_SlotIndex", Value: Số lượng ca đang có ở đó
        private Dictionary<string, int> _slotOccupancy = new Dictionary<string, int>();

        // Clipboard cho copy/cut/paste
        private ShiftData _clipboard = null;
        private bool _clipboardIsCut = false;

        // Drag ghost adorner
        private AdornerLayer _adornerLayer;
        private DragAdorner _dragAdorner;
        private double _adornerWidth;
        private double _adornerHeight;

        // Resize state
        private enum ResizeDirection { None, Left, Right }
        private ResizeDirection _currentResizeDir = ResizeDirection.None;
        private bool _isResizing = false;
        private Border _resizingBlock = null;
        private Point _resizeStartPoint;
        private int _origColumn;
        private int _origSpan;
        private TimeSpan _origStartTime;
        private TimeSpan _origEndTime;
        private double _slotPixelWidth = 60; // fallback, updated when drag starts

        // Vertical scroll lock threshold (pixels). Content cannot be scrolled above this offset.
        // Set to desired value (0 = topmost). Increase to prevent scrolling up further.
        private double _verticalLockOffset = 45.0; // lock at ~header height + small offset

        public SchedulePage()
        {
            InitializeComponent();
            _currentMonthDisplay = DateTime.Now; // Mặc định tháng hiện tại
            InitComboBoxes();
            LoadStaffNames(); // populate cbName
            RenderSchedule();

            // Prevent header grids from receiving focus (visual)
            HeaderScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LeftScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            // Attach wheel handler to lock upward scrolling when close to top using inline lambda
            ContentScrollViewer.PreviewMouseWheel += (s, e) =>
            {
                if (e.Delta > 0) // scrolling up
                {
                    if (ContentScrollViewer.VerticalOffset <= _verticalLockOffset + 0.5)
                    {
                        // lock: suppress further upward scrolling
                        e.Handled = true;
                        return;
                    }
                }
                // otherwise let it scroll normally
            };
        }

        private Brush GetBrushForUser(int userId)
        {
            if (_userBrushes.ContainsKey(userId)) return _userBrushes[userId];
            var brush = _palette[_nextPaletteIndex % _palette.Count];
            _userBrushes[userId] = brush;
            _nextPaletteIndex++;
            return brush;
        }

        private Brush GetContrastingForeground(Brush background)
        {
            if (background is SolidColorBrush sb)
            {
                var c = sb.Color;
                // luminance
                double lum = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
                return lum > 0.6 ? Brushes.Black : Brushes.White;
            }
            return Brushes.White;
        }

        private ComboBox GetNameCombo()
        {
            return this.FindName("cbName") as ComboBox;
        }

        private void LoadStaffNames()
        {
            try
            {
                var users = UserDAO.Instance.GetListUser();
                var staffUsers = users.Where(u => u.RoleName == "Staff" && u.IsActive).ToList();
                var cb = GetNameCombo();
                if (cb != null)
                {
                    cb.ItemsSource = staffUsers;
                    cb.DisplayMemberPath = "FullName";
                    if (staffUsers.Count > 0) cb.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void InitComboBoxes()
        {
            cbEndHour.Items.Clear();
            cbEndMinute.Items.Clear();
            for (int i = StartHour; i <= EndHour; i++) cbEndHour.Items.Add(i);
            cbEndMinute.Items.Add(0);
            cbEndMinute.Items.Add(30);
        }

        // --- NAVIGATION THÁNG ---
        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonthDisplay = _currentMonthDisplay.AddMonths(-1);
            RenderSchedule();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonthDisplay = _currentMonthDisplay.AddMonths(1);
            RenderSchedule();
        }

        // --- CORE: VẼ LỊCH THEO THÁNG ---
        private void RenderSchedule()
        {
            // Cập nhật text hiển thị tháng
            txtCurrentMonth.Text = $"Tháng {_currentMonthDisplay.Month}, {_currentMonthDisplay.Year}";

            // Clear existing grids
            ScheduleGrid.Children.Clear();
            ScheduleGrid.RowDefinitions.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();

            TimeHeaderGrid.Children.Clear();
            TimeHeaderGrid.ColumnDefinitions.Clear();
            TimeHeaderGrid.RowDefinitions.Clear();

            DateColumnGrid.Children.Clear();
            DateColumnGrid.RowDefinitions.Clear();

            int daysInMonth = DateTime.DaysInMonth(_currentMonthDisplay.Year, _currentMonthDisplay.Month);
            int totalSlots = (EndHour - StartHour) * (60 / StepMinutes);

            //1. TẠO CỘT
            // Các cột Slot (30p)
            for (int i = 0; i < totalSlots; i++)
            {
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

                // Also create matching columns in TimeHeaderGrid for alignment
                TimeHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            }

            //2. TẠO HÀNG
            ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Header
            // Add a row for TimeHeaderGrid as well
            TimeHeaderGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            // Add a spacer row at top of DateColumnGrid so its rows align with ScheduleGrid content rows
            DateColumnGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            for (int i = 1; i <= daysInMonth; i++)
            {
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

                // add rows to DateColumnGrid (after the spacer)
                DateColumnGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            }

            // Note: do not force explicit Width/Height on grids; let WPF measure them. We'll align via matching ColumnDefinitions and synchronized scrolling.

            //3. VẼ HEADER GIỜ (LOGIC MỚI - CLEAN CODE)
            // Build hour headers into TimeHeaderGrid
            // Chỉ chạy i < totalSlots (Không vẽ mốc cuối cùng22h)
            for (int i = 0; i < totalSlots; i += 2)
            {
                var time = new TimeSpan(StartHour, 0, 0).Add(TimeSpan.FromMinutes(i * StepMinutes));

                Border headerBorder = new Border
                {
                    Style = (Style)FindResource("HeaderBorderStyle"),
                    Width = 120, // Rộng120px (chiếm trọn1 tiếng)

                    // Căn Trái: Chữ bắt đầu ngay tại vạch kẻ
                    HorizontalAlignment = HorizontalAlignment.Left,

                    // Kéo lùi nhẹ15px để tâm chữ "06" nằm giữa vạch kẻ cho đẹp mắt (giống Google Calendar)
                    // Nếu bạn thích nó nằm hẳn bên trong ô thì sửa thành (5,0,0,0)
                    Margin = new Thickness(0, 0, 0, 0)
                };

                TextBlock txtTime = new TextBlock
                {
                    Text = time.ToString(@"hh\:mm"),
                    Style = (Style)FindResource("HeaderTextStyle"),
                    // Vì Border đã căn Left rồi, TextBlock chỉ cần Center trong Border hoặc Left tùy ý
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                headerBorder.Child = txtTime;

                // ĐỊNH VỊ: header aligns with ScheduleGrid columns (no leading date column)
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);

                // Luôn luôn Span2 cột (Vì ta đã bỏ mốc cuối cùng, nên các mốc còn lại luôn đủ chỗ chứa1 tiếng)
                Grid.SetColumnSpan(headerBorder, 2);

                // add vertical guideline into ScheduleGrid spanning the content rows (start at row1)
                Border guideLineContent = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                    BorderThickness = new Thickness(1, 0, 0, 0),
                    Opacity = 0.5

                };
                Grid.SetRow(guideLineContent, 1);
                Grid.SetRowSpan(guideLineContent, daysInMonth);
                Grid.SetColumn(guideLineContent, i);
                ScheduleGrid.Children.Add(guideLineContent);

                // Also add a small tick/line in header row for visual alignment
                Border guideLineHeader = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                    BorderThickness = new Thickness(1, 0, 0, 0),
                    Opacity = 0.5
                };
                Grid.SetRow(guideLineHeader, 0);
                Grid.SetRowSpan(guideLineHeader, 1);
                Grid.SetColumn(guideLineHeader, i);
                TimeHeaderGrid.Children.Add(guideLineHeader);

                TimeHeaderGrid.Children.Add(headerBorder);
            }

            //4. VẼ NGÀY VÀ CELL (Giữ nguyên logic cũ) but also fill DateColumnGrid
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(_currentMonthDisplay.Year, _currentMonthDisplay.Month, day);

                // Header Ngày
                Border dayHeader = new Border { Style = (Style)FindResource("HeaderBorderStyle"), Padding = new Thickness(10, 0, 0, 0) };
                StackPanel spDay = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

                Brush primaryColor = (Brush)TryFindResource("PrimaryColor") ?? Brushes.Black;

                TextBlock txtDayNum = new TextBlock
                {
                    Text = day.ToString("00"),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = primaryColor
                };
                TextBlock txtDayName = new TextBlock { Text = GetVietnameseDayName(date.DayOfWeek), FontSize = 11, Foreground = Brushes.Gray };

                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    txtDayNum.Foreground = Brushes.Red;
                    txtDayName.Foreground = Brushes.Red;
                }

                spDay.Children.Add(txtDayNum);
                spDay.Children.Add(txtDayName);
                dayHeader.Child = spDay;

                Grid.SetRow(dayHeader, day); // shift by1 because row0 is spacer/header
                Grid.SetColumn(dayHeader, 0);
                DateColumnGrid.Children.Add(dayHeader);

                // Cells
                for (int c = 0; c < totalSlots; c++)
                {
                    var currentTime = new TimeSpan(StartHour, 0, 0).Add(TimeSpan.FromMinutes(c * StepMinutes));
                    Border cell = new Border { Style = (Style)FindResource("GridCellStyle") };
                    cell.Tag = new { Date = date, Time = currentTime, ColIndex = c + 1 };
                    cell.MouseLeftButtonDown += Cell_Click;

                    // Allow drop for drag & drop
                    cell.AllowDrop = true;
                    cell.DragEnter += Cell_DragEnter;
                    cell.DragOver += Cell_DragOver;
                    cell.Drop += Cell_Drop;

                    // Right click for paste
                    cell.MouseRightButtonUp += Cell_RightClickForPaste;

                    Grid.SetRow(cell, day);
                    Grid.SetColumn(cell, c);
                    ScheduleGrid.Children.Add(cell);
                }

                // Load saved schedules for this date and render them
                try
                {
                    var schedules = WorkScheduleDAO.Instance.GetListByDate(date);
                    foreach (var s in schedules)
                    {
                        string name = UserDAO.Instance.GetFullName(s.UserId);
                        TimeSpan start = s.StartTime;
                        TimeSpan duration = s.EndTime - s.StartTime;
                        AddShiftToGrid(name, date, start, duration, s.Id, s.UserId);
                    }
                }
                catch { }
            }

            // After creating ColumnDefinitions and RowDefinitions in RenderSchedule(), set explicit sizes so header/date align with content

            // compute fixed pixel sizes (we use60 per time slot,50 header,60 per day)
            int slotPixelWidth = 60;
            int headerPixelHeight = 50;
            int dayRowHeight = 60;

            // total width for time columns
            double totalTimeWidth = totalSlots * slotPixelWidth;

            // apply widths/heights to ensure scroll extents match
            try
            {
                TimeHeaderGrid.Width = totalTimeWidth;
                ScheduleGrid.Width = totalTimeWidth;

                double totalContentHeight = headerPixelHeight + (daysInMonth * dayRowHeight);
                ScheduleGrid.Height = totalContentHeight;
                DateColumnGrid.Height = totalContentHeight;
            }
            catch { }

            // Note: ColumnDefinitions already created with fixed60 widths, and RowDefinitions with fixed50/60 heights above.
        }

        // Keep header and left column synchronized with main scrollviewer
        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Horizontal -> move header
            if (e.HorizontalChange != 0)
            {
                HeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }

            // Vertical -> move left column
            if (e.VerticalChange != 0)
            {
                LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            }

            // clamp vertical offset to lock threshold
            if (ContentScrollViewer.VerticalOffset < _verticalLockOffset)
            {
                // small delay to avoid reentrancy issues
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    ContentScrollViewer.ScrollToVerticalOffset(_verticalLockOffset);
                }));
            }

            // If vertical scrollbar is visible on content, add right padding to header to account for scrollbar width
            try
            {
                var vs = ContentScrollViewer.ComputedVerticalScrollBarVisibility;
                double padRight = (vs == Visibility.Visible) ? SystemParameters.VerticalScrollBarWidth : 0;
                HeaderScrollViewer.Padding = new Thickness(0, 0, padRight, 0);
            }
            catch { }
        }

        // Helper: Tên thứ tiếng Việt
        private string GetVietnameseDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Thứ 2";
                case DayOfWeek.Tuesday: return "Thứ 3";
                case DayOfWeek.Wednesday: return "Thứ 4";
                case DayOfWeek.Thursday: return "Thứ 5";
                case DayOfWeek.Friday: return "Thứ 6";
                case DayOfWeek.Saturday: return "Thứ 7";
                case DayOfWeek.Sunday: return "CN";
                default: return "";
            }
        }

        // --- SỰ KIỆN CLICK VÀO Ô ---
        private void Cell_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            dynamic data = border.Tag;
            _selectedDate = data.Date;
            _selectedStartTime = data.Time;

            // Fill dữ liệu vào Popup
            txtDate.Text = _selectedDate.ToString("dd/MM/yyyy");
            txtStartTime.Text = _selectedStartTime.ToString(@"hh\:mm");
            txtError.Text = string.Empty;

            // clear / reset name combo
            var cb = GetNameCombo();
            if (cb != null)
            {
                cb.SelectedIndex = -1;
                cb.Text = string.Empty;
                cb.Focus();
            }

            // Auto chọn giờ kết thúc (+1 tiếng)
            var endTime = _selectedStartTime.Add(TimeSpan.FromHours(1));
            if (endTime > new TimeSpan(EndHour, 0, 0)) endTime = new TimeSpan(EndHour, 0, 0);

            cbEndHour.SelectedItem = endTime.Hours;
            cbEndMinute.SelectedItem = endTime.Minutes;

            PopupOverlay.Visibility = Visibility.Visible;
            cbName.Focus();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var cb = GetNameCombo();
            if (cb == null) { txtError.Text = "Lỗi nội bộ"; return; }
            if (cb.SelectedItem == null) { txtError.Text = "Chọn nhân viên!"; return; }
            if (cbEndHour.SelectedItem == null) return;

            var selectedUser = cb.SelectedItem as User;
            if (selectedUser == null) { txtError.Text = "Nhân viên không hợp lệ."; return; }

            string name = selectedUser.FullName;
            int userId = selectedUser.Id;

            int endH = (int)cbEndHour.SelectedItem;
            int endM = (int)cbEndMinute.SelectedItem;
            TimeSpan endTime = new TimeSpan(endH, endM, 0);

            if (endTime <= _selectedStartTime) { txtError.Text = "Giờ kết thúc không hợp lệ."; return; }

            TimeSpan duration = endTime - _selectedStartTime;
            if (duration.TotalMinutes % 30 != 0) { txtError.Text = "Thời gian phải chẵn30p."; return; }

            // Check overlap in UI before saving
            int startSlot = (int)((_selectedStartTime.TotalMinutes - (StartHour * 60)) / StepMinutes);
            int span = (int)(duration.TotalMinutes / StepMinutes);
            if (!IsRangeFree(_selectedDate, startSlot, span, null))
            {
                txtError.Text = "Vùng thời gian đã có ca khác.";
                return;
            }

            // Save to database
            try
            {
                bool ok = WorkScheduleDAO.Instance.RegisterSchedule(userId, _selectedDate, _selectedStartTime, endTime, "");
                if (!ok) { txtError.Text = "Không thể lưu ca làm."; return; }
            }
            catch
            {
                txtError.Text = "Lỗi khi lưu dữ liệu.";
                return;
            }

            // Refresh view (re-render month) so saved shift appears
            RenderSchedule();
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        // --- VẼ KHỐI CA LÀM ---
        private void AddShiftToGrid(string name, DateTime date, TimeSpan start, TimeSpan duration, int scheduleId = -1, int userId = -1)
        {
            // Tìm dòng (Row) tương ứng với ngày
            // Vì Row 0 là Header nên Row index = Day
            int rowIndex = date.Day;

            // Tìm cột (Column)
            int startSlotIndex = (int)((start.TotalMinutes - (StartHour * 60)) / StepMinutes);
            int spanCount = (int)(duration.TotalMinutes / StepMinutes);

            Border shiftBlock = new Border
            {
                Style = (Style)FindResource("ShiftBlockStyle"),
                ToolTip = $"{name} ({start:hh\\:mm} - {start.Add(duration):hh\\:mm})",
            };

            TextBlock txtContent = new TextBlock
            {
                Text = $"{name}\n{start:hh\\:mm}-{start.Add(duration):hh\\:mm}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(67, 56, 202)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            shiftBlock.Child = txtContent;

            // Tag contains schedule metadata so we can manipulate it later
            shiftBlock.Tag = new ShiftData
            {
                ScheduleId = scheduleId,
                UserId = userId,
                UserName = name,
                Date = date,
                Start = start,
                End = start.Add(duration)
            };

            // Apply per-employee color
            if (userId > 0)
            {
                var bg = GetBrushForUser(userId);
                shiftBlock.Background = bg;
                // adjust text color for contrast
                txtContent.Foreground = GetContrastingForeground(bg);
            }
            else
            {
                // default text color
                txtContent.Foreground = new SolidColorBrush(Color.FromRgb(67, 56, 202));
            }

            // Interactivity
            shiftBlock.PreviewMouseLeftButtonDown += ShiftBlock_PreviewMouseLeftButtonDown;
            shiftBlock.MouseRightButtonUp += (s, e) =>
            {
                var border = s as Border;
                if (border == null) return;
                var data = border.Tag as ShiftData;
                if (data == null) return;

                ContextMenu cm = new ContextMenu();
                MenuItem miCopy = new MenuItem { Header = "Copy" };
                miCopy.Click += (ss, ev) => { _clipboard = data.Clone(); _clipboardIsCut = false; };
                MenuItem miCut = new MenuItem { Header = "Cut" };
                miCut.Click += (ss, ev) => { _clipboard = data.Clone(); _clipboardIsCut = true; };
                MenuItem miDelete = new MenuItem { Header = "Delete" };
                miDelete.Click += (ss, ev) =>
                {
                    if (data.ScheduleId > 0)
                    {
                        WorkScheduleDAO.Instance.DeleteSchedule(data.ScheduleId);
                        RenderSchedule();
                    }
                    else
                    {
                        try { ScheduleGrid.Children.Remove(border); } catch { }
                    }
                };
                cm.Items.Add(miCopy);
                cm.Items.Add(miCut);
                cm.Items.Add(new Separator());
                cm.Items.Add(miDelete);
                border.ContextMenu = cm;
                cm.IsOpen = true;
                e.Handled = true;
            };
            shiftBlock.MouseMove += ShiftBlock_MouseMove; // for cursor/resize feedback
            shiftBlock.PreviewMouseLeftButtonUp += ShiftBlock_PreviewMouseLeftButtonUp; // finalize resize
            shiftBlock.MouseLeave += ShiftBlock_MouseLeave; // reset cursor when leaving

            Grid.SetRow(shiftBlock, rowIndex);
            Grid.SetColumn(shiftBlock, startSlotIndex);
            Grid.SetColumnSpan(shiftBlock, spanCount);

            ScheduleGrid.Children.Add(shiftBlock);
        }

        // Show resize cursor when pointer near edges
        private void ShiftBlock_MouseMove(object sender, MouseEventArgs e)
        {
            var block = sender as Border;
            if (block == null) return;

            // If currently resizing, update size
            if (_isResizing && _resizingBlock == block)
            {
                HandleResizing(block, e);
                return;
            }

            // Otherwise, show cursor when near left/right edge
            Point pos = e.GetPosition(block);
            const double threshold = 10.0;
            if (pos.X <= threshold)
            {
                block.Cursor = Cursors.SizeWE;
                _currentResizeDir = ResizeDirection.Left;
            }
            else if (pos.X >= block.ActualWidth - threshold)
            {
                block.Cursor = Cursors.SizeWE;
                _currentResizeDir = ResizeDirection.Right;
            }
            else
            {
                block.Cursor = Cursors.Hand; // draggable
                _currentResizeDir = ResizeDirection.None;
            }
        }

        private void ShiftBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            var block = sender as Border;
            if (block == null) return;
            if (!_isResizing)
            {
                block.Cursor = Cursors.Arrow;
                _currentResizeDir = ResizeDirection.None;
            }
        }

        // Start drag or start resize depending on pointer location
        private void ShiftBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            var data = border.Tag as ShiftData;
            if (data == null) return;

            // If pointer was near edge -> begin resize
            Point posInBlock = e.GetPosition(border);
            const double threshold = 10.0;
            bool nearLeft = posInBlock.X <= threshold;
            bool nearRight = posInBlock.X >= border.ActualWidth - threshold;

            // Update slot pixel width from grid if possible
            if (ScheduleGrid.ColumnDefinitions.Count > 0)
            {
                _slotPixelWidth = ScheduleGrid.ColumnDefinitions[0].ActualWidth;
                if (_slotPixelWidth <= 0) _slotPixelWidth = 60;
            }

            if (nearLeft || nearRight)
            {
                // Start resizing
                _isResizing = true;
                _resizingBlock = border;
                _resizeStartPoint = e.GetPosition(ScheduleGrid);
                _origColumn = Grid.GetColumn(border);
                _origSpan = Grid.GetColumnSpan(border);
                _origStartTime = data.Start;
                _origEndTime = data.End;
                _currentResizeDir = nearLeft ? ResizeDirection.Left : ResizeDirection.Right;

                // capture mouse to keep receiving events
                border.CaptureMouse();
                e.Handled = true;
                return;
            }

            // Otherwise start normal drag (existing behavior)
            // Create ghost adorner
            try
            {
                _adornerLayer = AdornerLayer.GetAdornerLayer(ScheduleGrid);
                if (_adornerLayer != null)
                {
                    var brush = new VisualBrush(border) { Opacity = 0.9, Stretch = Stretch.None };
                    _adornerWidth = border.ActualWidth;
                    _adornerHeight = border.ActualHeight;
                    _dragAdorner = new DragAdorner(ScheduleGrid, brush, _adornerWidth, _adornerHeight);
                }
            }
            catch { /* ignore adorner errors */ }

            // Prepare drag data: include a reference to the source UI element so we can remove it on move
            var dragData = new ShiftData
            {
                ScheduleId = data.ScheduleId,
                UserId = data.UserId,
                UserName = data.UserName,
                Date = data.Date,
                Start = data.Start,
                End = data.End,
                SourceElement = border
            };

            // Start drag and allow copy/move
            DataObject dobj = new DataObject("ShiftData", dragData);
            var finalEffect = DragDrop.DoDragDrop(border, dobj, DragDropEffects.Move | DragDropEffects.Copy);

            // If the drag result was Move, remove original (DB row or UI element)
            if ((finalEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                // If original had DB id, delete it
                if (dragData.ScheduleId > 0)
                {
                    try { WorkScheduleDAO.Instance.DeleteSchedule(dragData.ScheduleId); } catch { }
                }
                else
                {
                    // remove UI element directly
                    if (dragData.SourceElement is UIElement src)
                    {
                        try { ScheduleGrid.Children.Remove(src); } catch { }
                    }
                }

                // Refresh after removal
                RenderSchedule();
            }

            // cleanup adorner after drag finished
            RemoveDragAdorner();
        }

        // Right-click on a shift block: copy/cut/delete
        private void ShiftBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            var data = border.Tag as ShiftData;
            if (data == null) return;

            ContextMenu cm = new ContextMenu();

            MenuItem miCopy = new MenuItem { Header = "Copy" };
            miCopy.Click += (s, ev) => { _clipboard = data.Clone(); _clipboardIsCut = false; };

            MenuItem miCut = new MenuItem { Header = "Cut" };
            miCut.Click += (s, ev) => { _clipboard = data.Clone(); _clipboardIsCut = true; };

            MenuItem miDelete = new MenuItem { Header = "Delete" };
            miDelete.Click += (s, ev) => {
                if (data.ScheduleId > 0)
                {
                    WorkScheduleDAO.Instance.DeleteSchedule(data.ScheduleId);
                    RenderSchedule();
                }
                else
                {
                    // remove UI element if not persisted
                    try { ScheduleGrid.Children.Remove(border); } catch { }
                }
            };

            cm.Items.Add(miCopy);
            cm.Items.Add(miCut);
            cm.Items.Add(new Separator());
            cm.Items.Add(miDelete);

            border.ContextMenu = cm;
            cm.IsOpen = true;
            e.Handled = true;
        }

        // Finalize resize on mouse up
        private void ShiftBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isResizing || _resizingBlock == null) return;

            // release capture
            try { _resizingBlock.ReleaseMouseCapture(); } catch { }

            // Compute final start/end based on current visual column/span
            int col = Grid.GetColumn(_resizingBlock);
            int span = Grid.GetColumnSpan(_resizingBlock);
            int startSlot = col; // columns are0-based slots
            TimeSpan newStart = TimeSpan.FromMinutes(StartHour * 60 + startSlot * StepMinutes);
            TimeSpan newEnd = newStart.Add(TimeSpan.FromMinutes(span * StepMinutes));

            var sd = _resizingBlock.Tag as ShiftData;
            if (sd != null && sd.ScheduleId > 0)
            {
                // Update DB
                try
                {
                    bool ok = WorkScheduleDAO.Instance.UpdateSchedule(sd.ScheduleId, sd.Date, newStart, newEnd);
                    if (!ok) MessageBox.Show("Cập nhật ca thất bại.");
                }
                catch { MessageBox.Show("Lỗi khi cập nhật ca."); }
            }
            else if (sd != null && sd.ScheduleId <= 0)
            {
                // No DB id - attempt to create new schedule and remove original (if needed)
                try
                {
                    bool ok = WorkScheduleDAO.Instance.RegisterSchedule(sd.UserId, sd.Date, newStart, newEnd, "");
                    if (!ok) MessageBox.Show("Lưu ca thất bại.");
                }
                catch { MessageBox.Show("Lỗi khi lưu ca."); }
            }

            // Reset state and refresh
            _isResizing = false;
            _currentResizeDir = ResizeDirection.None;
            _resizingBlock = null;
            RenderSchedule();
            e.Handled = true;
        }

        // Handle resizing while mouse moves
        private void HandleResizing(Border block, MouseEventArgs e)
        {
            if (!_isResizing || _resizingBlock != block) return;

            Point pos = e.GetPosition(ScheduleGrid);

            // If the mouse moved outside the schedule grid to the left (into the date column) or
            // beyond the right edge, ignore resize updates to avoid moving blocks into the date column.
            if (pos.X <0 || pos.X > ScheduleGrid.ActualWidth)
            {
                return;
            }

            double deltaX = pos.X - _resizeStartPoint.X;
            // compute slot delta (positive means moving right)
            int slotDelta = (int)Math.Round(deltaX / _slotPixelWidth);

            int origStartSlot = _origColumn; // columns are0-based, remove previous -1 offset

            if (_currentResizeDir == ResizeDirection.Left)
            {
                int newStartSlot = origStartSlot + slotDelta;
                int newSpan = _origSpan - slotDelta;

                // clamp
                if (newStartSlot <0)
                {
                    newStartSlot =0;
                    newSpan = (_origColumn - newStartSlot);
                }
                if (newSpan <1) newSpan =1;

                // check overlap ignoring this block
                if (!IsRangeFree(((ShiftData)block.Tag).Date, newStartSlot, newSpan, block))
                {
                    // do not apply change if it would overlap
                    return;
                }

                // update visual
                Grid.SetColumn(block, newStartSlot);
                Grid.SetColumnSpan(block, newSpan);

                // update tag times for live feedback
                var sd = block.Tag as ShiftData;
                if (sd != null)
                {
                    sd.Start = TimeSpan.FromMinutes(StartHour *60 + newStartSlot * StepMinutes);
                    sd.End = sd.Start.Add(TimeSpan.FromMinutes(newSpan * StepMinutes));
                    UpdateBlockText(block, sd);
                }
            }
            else if (_currentResizeDir == ResizeDirection.Right)
            {
                int newSpan = _origSpan + slotDelta;
                if (newSpan <1) newSpan =1;

                // ensure not beyond last slot
                int totalSlots = (EndHour - StartHour) * (60 / StepMinutes);
                int startSlot = origStartSlot;
                if (startSlot + newSpan > totalSlots) newSpan = totalSlots - startSlot;
                if (newSpan <1) newSpan =1;

                // check overlap ignoring this block
                if (!IsRangeFree(((ShiftData)block.Tag).Date, startSlot, newSpan, block))
                {
                    // do not apply change if it would overlap
                    return;
                }

                Grid.SetColumnSpan(block, newSpan);

                var sd = block.Tag as ShiftData;
                if (sd != null)
                {
                    sd.End = sd.Start.Add(TimeSpan.FromMinutes(newSpan * StepMinutes));
                    UpdateBlockText(block, sd);
                }
            }
        }
        private void UpdateBlockText(Border block, ShiftData sd)
        {
            if (block.Child is TextBlock tb && sd != null)
            {
                tb.Text = $"{sd.UserName}\n{sd.Start:hh\\:mm}-{sd.End:hh\\:mm}";
            }
        }

        // --- DRAG & DROP ON CELLS ---
        #region DragDrop on cells
        private void Cell_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ShiftData")) e.Effects = DragDropEffects.None;
        }

        private void Cell_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ShiftData")) { e.Effects = DragDropEffects.None; return; }
            e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
            e.Handled = true;

            // Update adorner position if exists
            UpdateDragAdornerPosition(e.GetPosition(ScheduleGrid));
        }

        private void Cell_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ShiftData")) return;
            var data = e.Data.GetData("ShiftData") as ShiftData;
            if (data == null) return;

            var cell = sender as Border;
            if (cell == null) return;
            dynamic tag = cell.Tag;
            DateTime targetDate = tag.Date;
            TimeSpan targetStart = tag.Time;

            // Keep same duration
            TimeSpan duration = data.End - data.Start;
            TimeSpan targetEnd = targetStart.Add(duration);
            if (targetEnd > new TimeSpan(EndHour, 0, 0))
            {
                MessageBox.Show("Không thể dán: vượt quá giờ làm.");
                RemoveDragAdorner();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Check overlap: ignore source element if present (moving)
            int startSlot = (int)((targetStart.TotalMinutes - (StartHour * 60)) / StepMinutes);
            int span = (int)(duration.TotalMinutes / StepMinutes);
            UIElement ignoreEl = data.SourceElement as UIElement;
            if (!IsRangeFree(targetDate, startSlot, span, ignoreEl))
            {
                MessageBox.Show("Vị trí dán bị chồng lấp với ca khác.");
                RemoveDragAdorner();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Insert new schedule first
            bool inserted = WorkScheduleDAO.Instance.RegisterSchedule(data.UserId, targetDate, targetStart, targetEnd, "");
            if (!inserted)
            {
                MessageBox.Show("Lưu ca mới thất bại.");
                RemoveDragAdorner();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Determine whether the source requested copy or move (Ctrl key) and set the final effect accordingly.
            bool sourceRequestedCopy = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            e.Effects = sourceRequestedCopy ? DragDropEffects.Copy : DragDropEffects.Move;
            e.Handled = true;

            // Refresh to show inserted schedule
            RenderSchedule();
            RemoveDragAdorner();
        }
        #endregion

        // Right click on a cell to paste from clipboard
        private void Cell_RightClickForPaste(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as Border;
            if (cell == null) return;
            ContextMenu cm = new ContextMenu();

            MenuItem miPaste = new MenuItem { Header = "Paste" };
            miPaste.IsEnabled = _clipboard != null;
            miPaste.Click += (s, ev) =>
            {
                if (_clipboard == null) return;
                dynamic tag = cell.Tag;
                DateTime targetDate = tag.Date;
                TimeSpan targetStart = tag.Time;
                TimeSpan duration = _clipboard.End - _clipboard.Start;
                TimeSpan targetEnd = targetStart.Add(duration);
                if (targetEnd > new TimeSpan(EndHour, 0, 0)) { MessageBox.Show("Không thể dán: vượt quá giờ làm."); return; }

                int startSlot = (int)((targetStart.TotalMinutes - (StartHour * 60)) / StepMinutes);
                int span = (int)(duration.TotalMinutes / StepMinutes);
                if (!IsRangeFree(targetDate, startSlot, span, null)) { MessageBox.Show("Vị trí dán bị chồng lấp với ca khác."); return; }

                // Insert
                bool ok = WorkScheduleDAO.Instance.RegisterSchedule(_clipboard.UserId, targetDate, targetStart, targetEnd, "");
                if (!ok) { MessageBox.Show("Lưu ca thất bại."); return; }

                // If it was cut, delete original
                if (_clipboardIsCut && _clipboard.ScheduleId > 0)
                {
                    WorkScheduleDAO.Instance.DeleteSchedule(_clipboard.ScheduleId);
                    _clipboard = null;
                    _clipboardIsCut = false;
                }

                RenderSchedule();
            };

            cm.Items.Add(miPaste);
            cell.ContextMenu = cm;
            cm.IsOpen = true;
            e.Handled = true;
        }

        // Update adorner position helper
        private void UpdateDragAdornerPosition(Point pos)
        {
            if (_dragAdorner == null) return;
            // place center of adorner at mouse
            double left = pos.X - _adornerWidth / 2;
            double top = pos.Y - _adornerHeight / 2;
            _dragAdorner.SetPosition(left, top);
        }

        private void RemoveDragAdorner()
        {
            try
            {
                if (_dragAdorner != null && _adornerLayer != null)
                {
                    _adornerLayer.Remove(_dragAdorner);
                }
            }
            catch { }
            _dragAdorner = null;
            _adornerLayer = null;
        }

        // Simple model for drag/drop clipboard
        private class ShiftData
        {
            public int ScheduleId { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }

            // reference to the originating UI element for immediate removal on move
            public UIElement SourceElement { get; set; }

            public ShiftData Clone()
            {
                return new ShiftData
                {
                    ScheduleId = this.ScheduleId,
                    UserId = this.UserId,
                    UserName = this.UserName,
                    Date = this.Date,
                    Start = this.Start,
                    End = this.End,
                    // note: do not clone SourceElement here to avoid holding stale ref
                    SourceElement = null
                };
            }
        }

        // Adorner that draws a VisualBrush of the dragged element
        private class DragAdorner : Adorner
        {
            private readonly VisualBrush _brush;
            private double _left;
            private double _top;
            private readonly double _width;
            private readonly double _height;

            public DragAdorner(UIElement adornedElement, VisualBrush brush, double width, double height)
                : base(adornedElement)
            {
                _brush = brush;
                _width = width;
                _height = height;
                IsHitTestVisible = false;
                var layer = AdornerLayer.GetAdornerLayer(adornedElement);
                layer?.Add(this);
            }

            public void SetPosition(double left, double top)
            {
                _left = left;
                _top = top;
                this.InvalidateVisual();
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                if (_brush != null)
                {
                    Rect rect = new Rect(new Point(_left, _top), new Size(_width, _height));
                    drawingContext.PushOpacity(0.95);
                    drawingContext.DrawRoundedRectangle(_brush, new Pen(Brushes.Gray, 1), rect, 6, 6);
                    drawingContext.Pop();
                }
            }
        }

        // Check whether the target slot range for a given date is free (no other shift blocks overlap).
        // If ignoreElement is provided, that UIElement will be skipped from the overlap check (useful when resizing/moving the same block).
        private bool IsRangeFree(DateTime date, int startSlot, int span, UIElement ignoreElement)
        {
            if (span <= 0) return false;
            int endSlotExclusive = startSlot + span; // exclusive
            foreach (UIElement child in ScheduleGrid.Children)
            {
                if (child == ignoreElement) continue;
                if (child is Border b && b.Tag is ShiftData sd)
                {
                    // Only consider blocks on the same date
                    if (sd.Date.Date != date.Date) continue;

                    int childStart = Grid.GetColumn(b);
                    int childSpan = Grid.GetColumnSpan(b);
                    int childEndExclusive = childStart + childSpan;

                    // ranges [startSlot, endSlotExclusive) and [childStart, childEndExclusive) overlap if
                    // startSlot < childEndExclusive && childStart < endSlotExclusive
                    if (startSlot < childEndExclusive && childStart < endSlotExclusive)
                    {
                        return false; // overlap found
                    }
                }
            }
            return true;
        }

        // Add the following method to delete all schedules for current month (with confirmation)
        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Xóa tất cả ca trong tháng {_currentMonthDisplay:MM/yyyy}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            DeleteAllSchedulesForCurrentMonth();
        }

        private void DeleteAllSchedulesForCurrentMonth()
        {
            // iterate each day and remove schedules
            int days = DateTime.DaysInMonth(_currentMonthDisplay.Year, _currentMonthDisplay.Month);
            for (int d =1; d <= days; d++)
            {
                var date = new DateTime(_currentMonthDisplay.Year, _currentMonthDisplay.Month, d);
                try
                {
                    var schedules = WorkScheduleDAO.Instance.GetListByDate(date);
                    foreach (var s in schedules)
                    {
                        try { WorkScheduleDAO.Instance.DeleteSchedule(s.Id); } catch { }
                    }
                }
                catch { }
            }

            // reload UI
            RenderSchedule();
        }
    }
}