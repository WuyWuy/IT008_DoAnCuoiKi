using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;

namespace QuanLyCaPhe.Views.SharedPage
{
    public partial class SchedulePage : Page
    {
        // --- 1. CẤU HÌNH & BIẾN TOÀN CỤC ---
        private const int StartHour = 6;
        private const int EndHour = 22;
        private const int StepMinutes = 30;

        private Dictionary<int, Brush> _userBrushes = new Dictionary<int, Brush>();
        private List<Brush> _palette = new List<Brush>
        {
            Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Yellow, Brushes.Pink,
            Brushes.Gray, Brushes.Aqua, Brushes.Orange, Brushes.Purple, Brushes.Teal
        };
        private int _nextPaletteIndex = 0;

        private DateTime _currentMonthDisplay;
        private DateTime _selectedDate;
        private TimeSpan _selectedStartTime;

        // --- CLIPBOARD (FIX LỖI DUPLICATE) ---
        private ShiftData _clipboard = null;
        private bool _clipboardIsCut = false;
        private bool _isProcessingPaste = false; // Khóa chống spam lệnh Paste

        // --- DRAG & DROP UI ---
        private AdornerLayer _adornerLayer;
        private DragAdorner _dragAdorner;
        private double _adornerWidth, _adornerHeight;

        // --- RESIZE ---
        private enum ResizeDirection { None, Left, Right }
        private ResizeDirection _currentResizeDir = ResizeDirection.None;
        private bool _isResizing = false;
        private Border _resizingBlock = null;
        private Point _resizeStartPoint;
        private int _origColumn, _origSpan;
        private double _slotPixelWidth = 60;

        public SchedulePage()
        {
            InitializeComponent();
            _currentMonthDisplay = DateTime.Now;

            RenderSchedule();

            // Cấu hình ScrollBar ẩn cho Header/Left (đồng bộ sau)
            HeaderScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LeftScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            // [QUAN TRỌNG] Cấu hình ScrollViewer chính
            // PanningMode="Both" -> Bắt buộc để Touchpad hoạt động mượt (Native)
            ContentScrollViewer.PanningMode = PanningMode.Both;
            ContentScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            ContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ContentScrollViewer.CanContentScroll = false; // Scroll theo pixel (mượt) thay vì theo dòng (giật)

            // Đăng ký sự kiện lăn chuột thông minh
            ContentScrollViewer.PreviewMouseWheel += ContentScrollViewer_PreviewMouseWheel;
        }


        // --- 2. XỬ LÝ SCROLL THÔNG MINH (MOUSE vs TOUCHPAD) ---

        private void ContentScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                double scrollFactor = 0.6;
                ContentScrollViewer.ScrollToHorizontalOffset(ContentScrollViewer.HorizontalOffset - e.Delta * scrollFactor);
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }


        // --- 3. RENDER LỊCH (GIỮ NGUYÊN) ---
        // (Phần này chỉ vẽ giao diện, không ảnh hưởng logic scroll/drag)

        private void RenderSchedule()
        {
            txtCurrentMonth.Text = $"Tháng {_currentMonthDisplay.Month}, {_currentMonthDisplay.Year}";

            ScheduleGrid.Children.Clear(); ScheduleGrid.RowDefinitions.Clear(); ScheduleGrid.ColumnDefinitions.Clear();
            TimeHeaderGrid.Children.Clear(); TimeHeaderGrid.ColumnDefinitions.Clear(); TimeHeaderGrid.RowDefinitions.Clear();
            DateColumnGrid.Children.Clear(); DateColumnGrid.RowDefinitions.Clear();

            int daysInMonth = DateTime.DaysInMonth(_currentMonthDisplay.Year, _currentMonthDisplay.Month);
            int totalSlots = (EndHour - StartHour) * (60 / StepMinutes);

            // Grid Defs
            for (int i = 0; i < totalSlots; i++)
            {
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                TimeHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            }

            // [FIX] Chỉ TimeHeaderGrid mới cần dòng Header (50px).
            // ScheduleGrid và DateColumnGrid bỏ dòng 50px đầu tiên đi để tránh bị thừa khoảng trống.
            TimeHeaderGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            for (int i = 1; i <= daysInMonth; i++)
            {
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
                DateColumnGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            }

            // Header Giờ & Đường kẻ dọc
            for (int i = 0; i < totalSlots; i += 2)
            {
                var time = new TimeSpan(StartHour, 0, 0).Add(TimeSpan.FromMinutes(i * StepMinutes));
                Border headerBorder = new Border { Style = (Style)FindResource("HeaderBorderStyle"), Width = 120, HorizontalAlignment = HorizontalAlignment.Left };
                headerBorder.Child = new TextBlock { Text = time.ToString(@"hh\:mm"), Style = (Style)FindResource("HeaderTextStyle"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };

                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                Grid.SetColumnSpan(headerBorder, 2);
                TimeHeaderGrid.Children.Add(headerBorder);

                Border guideLine = new Border { BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)), BorderThickness = new Thickness(1, 0, 0, 0), Opacity = 0.5 };

                // [FIX] Đổi Row từ 1 thành 0 (vì đã xóa dòng header rỗng)
                Grid.SetRow(guideLine, 0);
                Grid.SetRowSpan(guideLine, daysInMonth);
                Grid.SetColumn(guideLine, i);
                ScheduleGrid.Children.Add(guideLine);
            }

            // Body
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(_currentMonthDisplay.Year, _currentMonthDisplay.Month, day);

                // Cột Ngày
                Border dayHeader = new Border { Style = (Style)FindResource("HeaderBorderStyle"), Padding = new Thickness(10, 0, 0, 0) };
                StackPanel spDay = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                Brush primaryColor = (Brush)TryFindResource("PrimaryColor") ?? Brushes.Black;
                TextBlock txtDayNum = new TextBlock { Text = day.ToString("00"), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = primaryColor };
                TextBlock txtDayName = new TextBlock { Text = GetVietnameseDayName(date.DayOfWeek), FontSize = 11, Foreground = Brushes.Gray };
                if (date.DayOfWeek == DayOfWeek.Sunday) { txtDayNum.Foreground = Brushes.Red; txtDayName.Foreground = Brushes.Red; }
                spDay.Children.Add(txtDayNum); spDay.Children.Add(txtDayName);
                dayHeader.Child = spDay;

                // [FIX] Đổi Row từ "day" thành "day - 1" (Index bắt đầu từ 0)
                Grid.SetRow(dayHeader, day - 1);
                Grid.SetColumn(dayHeader, 0);
                DateColumnGrid.Children.Add(dayHeader);

                // Cells
                for (int c = 0; c < totalSlots; c++)
                {
                    var currentTime = new TimeSpan(StartHour, 0, 0).Add(TimeSpan.FromMinutes(c * StepMinutes));
                    Border cell = new Border { Style = (Style)FindResource("GridCellStyle") };
                    cell.Tag = new { Date = date, Time = currentTime, ColIndex = c + 1 };

                    cell.AllowDrop = true;
                    cell.PreviewMouseLeftButtonDown += Cell_PreviewMouseDown;
                    cell.DragEnter += Cell_DragEnter;
                    cell.DragOver += Cell_DragOver;
                    cell.Drop += Cell_Drop;
                    cell.MouseRightButtonUp += Cell_RightClickForPaste;

                    // [FIX] Đổi Row từ "day" thành "day - 1"
                    Grid.SetRow(cell, day - 1);
                    Grid.SetColumn(cell, c);
                    ScheduleGrid.Children.Add(cell);
                }

                try
                {
                    var schedules = WorkScheduleDAO.Instance.GetListByDate(date);
                    foreach (var s in schedules)
                    {
                        string name = UserDAO.Instance.GetFullName(s.UserId);
                        AddShiftToGrid(name, date, s.StartTime, s.EndTime - s.StartTime, s.Id, s.UserId);
                    }
                }
                catch { }
            }

            double totalTimeWidth = totalSlots * 60;
            TimeHeaderGrid.Width = totalTimeWidth;
            ScheduleGrid.Width = totalTimeWidth;

            // [FIX] Tính lại chiều cao (bỏ đi 50px thừa)
            double totalContentHeight = daysInMonth * 60;
            ScheduleGrid.Height = totalContentHeight;
            DateColumnGrid.Height = totalContentHeight;
        }

        private void Cell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = sender as Border;
                if (border == null) return;
                dynamic data = border.Tag;
                _selectedDate = data.Date;
                _selectedStartTime = data.Time;
                var detailWindow = new ScheduleRegisterWindow(_selectedDate, _selectedStartTime);
                if (detailWindow.ShowDialog() == true) RenderSchedule();
                e.Handled = true;
            }
        }


        // --- 4. LOGIC PASTE & COPY (AN TOÀN TUYỆT ĐỐI) ---

        // --- 6. MENU CHUỘT PHẢI (COPY/PASTE/DELETE) ---

        private void ShiftBlock_RightClick(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var data = border?.Tag as ShiftData;
            if (data == null) return;

            ContextMenu cm = new ContextMenu();
            // [QUAN TRỌNG] Áp dụng Style Glass
            cm.Style = (Style)FindResource("GlassContextMenu");

            // Thêm Icon (Dùng ký tự Unicode hoặc Path nếu muốn)
            MenuItem miCopy = new MenuItem { Header = "Sao chép (Copy)", Icon = new TextBlock { Text = "📄" } };
            miCopy.Click += (s, ev) => { _clipboard = data.Clone(); _clipboardIsCut = false; };

            MenuItem miCut = new MenuItem { Header = "Cắt (Cut)", Icon = new TextBlock { Text = "✂️" } };
            miCut.Click += (s, ev) => { _clipboard = data.Clone(); _clipboardIsCut = true; };

            MenuItem miDelete = new MenuItem { Header = "Xóa", Icon = new TextBlock { Text = "🗑️", Foreground = Brushes.Red } };
            miDelete.Click += (s, ev) =>
            {
                if (data.ScheduleId > 0) WorkScheduleDAO.Instance.DeleteSchedule(data.ScheduleId);
                RenderSchedule();
            };

            cm.Items.Add(miCopy);
            cm.Items.Add(miCut);
            cm.Items.Add(new Separator());
            cm.Items.Add(miDelete);

            border.ContextMenu = cm;
            cm.IsOpen = true;
            e.Handled = true;
        }

        private void Cell_RightClickForPaste(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as Border;
            if (cell == null) return;

            ContextMenu cm = new ContextMenu();
            // [QUAN TRỌNG] Áp dụng Style Glass
            cm.Style = (Style)FindResource("GlassContextMenu");

            MenuItem miPaste = new MenuItem { Header = "Dán (Paste)", IsEnabled = _clipboard != null, Icon = new TextBlock { Text = "📋" } };

            miPaste.Click += (s, ev) =>
            {
                // [Giữ nguyên logic Paste cũ của bạn]
                if (_clipboard == null || _isProcessingPaste) return;
                _isProcessingPaste = true;

                try
                {
                    dynamic tag = cell.Tag;
                    DateTime targetDate = tag.Date;
                    TimeSpan targetStart = tag.Time;
                    TimeSpan duration = _clipboard.End - _clipboard.Start;
                    TimeSpan targetEnd = targetStart.Add(duration);

                    if (_clipboardIsCut && _clipboard.ScheduleId > 0) WorkScheduleDAO.Instance.DeleteSchedule(_clipboard.ScheduleId);
                    WorkScheduleDAO.Instance.RegisterSchedule(_clipboard.UserId, targetDate, targetStart, targetEnd, "");

                    if (_clipboardIsCut) { _clipboard = null; _clipboardIsCut = false; }
                }
                finally
                {
                    _isProcessingPaste = false;
                    RenderSchedule();
                }
            };

            cm.Items.Add(miPaste);
            cell.ContextMenu = cm;
            cm.IsOpen = true;
            e.Handled = true;
        }

        private void Cell_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ShiftData")) return;
            var data = e.Data.GetData("ShiftData") as ShiftData;
            var cell = sender as Border;
            if (cell == null || data == null) return;

            dynamic tag = cell.Tag;
            DateTime targetDate = tag.Date;
            TimeSpan targetStart = tag.Time;
            TimeSpan duration = data.End - data.Start;
            TimeSpan targetEnd = targetStart.Add(duration);

            if (targetEnd > new TimeSpan(EndHour, 0, 0)) { MessageBox.Show("Vượt quá giờ làm!"); return; }

            int startSlot = (int)((targetStart.TotalMinutes - (StartHour * 60)) / StepMinutes);
            int span = (int)(duration.TotalMinutes / StepMinutes);
            if (!IsRangeFree(targetDate, startSlot, span, data.SourceElement as UIElement)) { MessageBox.Show("Bị trùng ca!"); return; }

            // [FIX] Logic Move/Copy rõ ràng
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            // Xóa cũ nếu là MOVE (không giữ Ctrl) và có ID
            if (!isCtrl && data.ScheduleId > 0)
                WorkScheduleDAO.Instance.DeleteSchedule(data.ScheduleId);

            // Tạo mới
            WorkScheduleDAO.Instance.RegisterSchedule(data.UserId, targetDate, targetStart, targetEnd, "");

            RenderSchedule();
            RemoveDragAdorner();
            e.Handled = true;
        }

        // --- CÁC HÀM HỖ TRỢ KHÁC ---

        private void AddShiftToGrid(string name, DateTime date, TimeSpan start, TimeSpan duration, int scheduleId = -1, int userId = -1)
        {
            int rowIndex = date.Day - 1;
            int startSlotIndex = (int)((start.TotalMinutes - (StartHour * 60)) / StepMinutes);
            int spanCount = (int)(duration.TotalMinutes / StepMinutes);

            Border shiftBlock = new Border
            {
                Style = (Style)FindResource("ShiftBlockStyle"),
                ToolTip = $"{name} ({start:hh\\:mm} - {start.Add(duration):hh\\:mm})",
                Tag = new ShiftData { ScheduleId = scheduleId, UserId = userId, UserName = name, Date = date, Start = start, End = start.Add(duration) }
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

            if (userId > 0) { var bg = GetBrushForUser(userId); shiftBlock.Background = bg; txtContent.Foreground = GetContrastingForeground(bg); }

            shiftBlock.PreviewMouseLeftButtonDown += ShiftBlock_PreviewMouseLeftButtonDown;
            shiftBlock.MouseMove += ShiftBlock_MouseMove;
            shiftBlock.MouseLeave += ShiftBlock_MouseLeave;
            shiftBlock.PreviewMouseLeftButtonUp += ShiftBlock_PreviewMouseLeftButtonUp;
            shiftBlock.MouseRightButtonUp += ShiftBlock_RightClick;

            Grid.SetRow(shiftBlock, rowIndex); Grid.SetColumn(shiftBlock, startSlotIndex); Grid.SetColumnSpan(shiftBlock, spanCount);
            ScheduleGrid.Children.Add(shiftBlock);
        }

        // Logic Resize & Drag cũ (Giữ nguyên)
        private void ShiftBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            var data = border.Tag as ShiftData;

            Point posInBlock = e.GetPosition(border);
            bool nearLeft = posInBlock.X <= 10;
            bool nearRight = posInBlock.X >= border.ActualWidth - 10;
            if (ScheduleGrid.ColumnDefinitions.Count > 0) _slotPixelWidth = ScheduleGrid.ColumnDefinitions[0].ActualWidth;

            if (nearLeft || nearRight)
            {
                _isResizing = true; _resizingBlock = border; _resizeStartPoint = e.GetPosition(ScheduleGrid);
                _origColumn = Grid.GetColumn(border); _origSpan = Grid.GetColumnSpan(border);
                _currentResizeDir = nearLeft ? ResizeDirection.Left : ResizeDirection.Right;
                border.CaptureMouse(); e.Handled = true;
            }
            else
            {
                try { _adornerLayer = AdornerLayer.GetAdornerLayer(ScheduleGrid); var brush = new VisualBrush(border) { Opacity = 0.9, Stretch = Stretch.None }; _dragAdorner = new DragAdorner(ScheduleGrid, brush, border.ActualWidth, border.ActualHeight); } catch { }
                var dragData = new ShiftData { ScheduleId = data.ScheduleId, UserId = data.UserId, Date = data.Date, Start = data.Start, End = data.End, SourceElement = border };
                DataObject dobj = new DataObject("ShiftData", dragData);
                DragDrop.DoDragDrop(border, dobj, DragDropEffects.Move | DragDropEffects.Copy);
                RemoveDragAdorner(); e.Handled = true;
            }
        }

        private void ShiftBlock_MouseMove(object sender, MouseEventArgs e)
        {
            var block = sender as Border; if (block == null) return;
            if (_isResizing && _resizingBlock == block) { HandleResizing(block, e); return; }
            Point pos = e.GetPosition(block);
            if (pos.X <= 10 || pos.X >= block.ActualWidth - 10) block.Cursor = Cursors.SizeWE; else block.Cursor = Cursors.Hand;
        }

        private void HandleResizing(Border block, MouseEventArgs e)
        {
            if (!_isResizing || _resizingBlock != block) return;
            Point pos = e.GetPosition(ScheduleGrid);
            double deltaX = pos.X - _resizeStartPoint.X;
            int slotDelta = (int)Math.Round(deltaX / _slotPixelWidth);

            if (_currentResizeDir == ResizeDirection.Left)
            {
                int newStartSlot = _origColumn + slotDelta; int newSpan = _origSpan - slotDelta;
                if (newStartSlot < 0) { newStartSlot = 0; newSpan = _origColumn; }
                if (newSpan < 1) newSpan = 1;
                if (IsRangeFree(((ShiftData)block.Tag).Date, newStartSlot, newSpan, block)) { Grid.SetColumn(block, newStartSlot); Grid.SetColumnSpan(block, newSpan); UpdateBlockTag(block, newStartSlot, newSpan); }
            }
            else
            {
                int newSpan = _origSpan + slotDelta; if (newSpan < 1) newSpan = 1;
                if (IsRangeFree(((ShiftData)block.Tag).Date, _origColumn, newSpan, block)) { Grid.SetColumnSpan(block, newSpan); UpdateBlockTag(block, _origColumn, newSpan); }
            }
        }

        private void UpdateBlockTag(Border block, int startSlot, int span)
        {
            var sd = block.Tag as ShiftData;
            if (sd != null) { sd.Start = TimeSpan.FromMinutes(StartHour * 60 + startSlot * StepMinutes); sd.End = sd.Start.Add(TimeSpan.FromMinutes(span * StepMinutes)); if (block.Child is TextBlock tb) tb.Text = $"{sd.UserName}\n{sd.Start:hh\\:mm}-{sd.End:hh\\:mm}"; }
        }

        private void ShiftBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isResizing || _resizingBlock == null) return;
            _resizingBlock.ReleaseMouseCapture();
            var sd = _resizingBlock.Tag as ShiftData;
            if (sd != null) { if (sd.ScheduleId > 0) WorkScheduleDAO.Instance.UpdateSchedule(sd.ScheduleId, sd.Date, sd.Start, sd.End); else WorkScheduleDAO.Instance.RegisterSchedule(sd.UserId, sd.Date, sd.Start, sd.End, ""); }
            _isResizing = false; _resizingBlock = null; _currentResizeDir = ResizeDirection.None; RenderSchedule();
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e) { if (MessageBox.Show($"Xóa tất cả ca trong tháng {_currentMonthDisplay:MM/yyyy}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) DeleteAllSchedulesForCurrentMonth(); }
        private void PrevMonth_Click(object sender, RoutedEventArgs e) { _currentMonthDisplay = _currentMonthDisplay.AddMonths(-1); RenderSchedule(); }
        private void NextMonth_Click(object sender, RoutedEventArgs e) { _currentMonthDisplay = _currentMonthDisplay.AddMonths(1); RenderSchedule(); }

        private void DeleteAllSchedulesForCurrentMonth()
        {
            int days = DateTime.DaysInMonth(_currentMonthDisplay.Year, _currentMonthDisplay.Month);
            for (int d = 1; d <= days; d++) { var date = new DateTime(_currentMonthDisplay.Year, _currentMonthDisplay.Month, d); var list = WorkScheduleDAO.Instance.GetListByDate(date); foreach (var s in list) WorkScheduleDAO.Instance.DeleteSchedule(s.Id); }
            RenderSchedule();
        }

        private bool IsRangeFree(DateTime date, int startSlot, int span, UIElement ignoreElement)
        {
            if (span <= 0) return false; int endSlot = startSlot + span;
            foreach (UIElement child in ScheduleGrid.Children) { if (child == ignoreElement) continue; if (child is Border b && b.Tag is ShiftData sd) { if (sd.Date.Date != date.Date) continue; int childStart = Grid.GetColumn(b); int childEnd = childStart + Grid.GetColumnSpan(b); if (startSlot < childEnd && childStart < endSlot) return false; } }
            return true;
        }

        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) { if (e.HorizontalChange != 0) HeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset); if (e.VerticalChange != 0) LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset); }
        private string GetVietnameseDayName(DayOfWeek day) { switch (day) { case DayOfWeek.Monday: return "Thứ 2"; case DayOfWeek.Tuesday: return "Thứ 3"; case DayOfWeek.Wednesday: return "Thứ 4"; case DayOfWeek.Thursday: return "Thứ 5"; case DayOfWeek.Friday: return "Thứ 6"; case DayOfWeek.Saturday: return "Thứ 7"; case DayOfWeek.Sunday: return "CN"; default: return ""; } }
        private Brush GetBrushForUser(int userId) { if (_userBrushes.ContainsKey(userId)) return _userBrushes[userId]; var brush = _palette[_nextPaletteIndex % _palette.Count]; _userBrushes[userId] = brush; _nextPaletteIndex++; return brush; }
        private Brush GetContrastingForeground(Brush background) { if (background is SolidColorBrush sb) { double lum = (0.299 * sb.Color.R + 0.587 * sb.Color.G + 0.114 * sb.Color.B) / 255.0; return lum > 0.6 ? Brushes.Black : Brushes.White; } return Brushes.White; }
        private void Cell_DragEnter(object sender, DragEventArgs e) { if (!e.Data.GetDataPresent("ShiftData")) e.Effects = DragDropEffects.None; }
        private void Cell_DragOver(object sender, DragEventArgs e) { if (!e.Data.GetDataPresent("ShiftData")) { e.Effects = DragDropEffects.None; return; } e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move; e.Handled = true; UpdateDragAdornerPosition(e.GetPosition(ScheduleGrid)); }
        private void ShiftBlock_MouseLeave(object sender, MouseEventArgs e) { if (!_isResizing) (sender as Border).Cursor = Cursors.Arrow; }
        private void UpdateDragAdornerPosition(Point pos) { if (_dragAdorner != null) _dragAdorner.SetPosition(pos.X - _adornerWidth / 2, pos.Y - _adornerHeight / 2); }
        private void RemoveDragAdorner() { if (_dragAdorner != null && _adornerLayer != null) _adornerLayer.Remove(_dragAdorner); _dragAdorner = null; }

        private class ShiftData { public int ScheduleId { get; set; } public int UserId { get; set; } public string UserName { get; set; } public DateTime Date { get; set; } public TimeSpan Start { get; set; } public TimeSpan End { get; set; } public UIElement SourceElement { get; set; } public ShiftData Clone() => new ShiftData { ScheduleId = ScheduleId, UserId = UserId, UserName = UserName, Date = Date, Start = Start, End = End }; }
        private class DragAdorner : Adorner { private readonly VisualBrush _brush; private double _left, _top, _width, _height; public DragAdorner(UIElement adornedElement, VisualBrush brush, double width, double height) : base(adornedElement) { _brush = brush; _width = width; _height = height; IsHitTestVisible = false; AdornerLayer.GetAdornerLayer(adornedElement)?.Add(this); } public void SetPosition(double left, double top) { _left = left; _top = top; InvalidateVisual(); } protected override void OnRender(DrawingContext drawingContext) { if (_brush != null) { drawingContext.PushOpacity(0.7); drawingContext.DrawRoundedRectangle(_brush, new Pen(Brushes.Gray, 1), new Rect(_left, _top, _width, _height), 6, 6); drawingContext.Pop(); } } }
    }
}