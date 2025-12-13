using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyCaPhe.Views.Components
{
    public partial class QuantitySelector : UserControl
    {
        public QuantitySelector()
        {
            InitializeComponent();
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(QuantitySelector),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as QuantitySelector)?.RaiseValueChanged();
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(QuantitySelector));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private void RaiseValueChanged()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(QuantitySelector.ValueChangedEvent, this);
            RaiseEvent(newEventArgs);
        }

        public int Minimum { get; set; } = 1;
        public int Maximum { get; set; } = 999;

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum) Value++;
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum) Value--;
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsLoaded) return;

            if (string.IsNullOrEmpty(InputBox.Text))
            {
                Value = Minimum;
                return;
            }

            if (int.TryParse(InputBox.Text, out int result))
            {
                if (result < Minimum)
                {
                    Value = Minimum;
                    InputBox.Text = Minimum.ToString();
                }
                else if (result > Maximum)
                {
                    Value = Maximum;
                    InputBox.Text = Maximum.ToString();
                }
                else
                {
                    Value = result;
                }
            }
        }
    }
}