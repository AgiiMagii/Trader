using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static Trader.Lib.Enums;

namespace Trader.Lib
{
    public class MessageService
    {
        private readonly Panel _messagePanel;
        private readonly Dictionary<MessageType, Brush> _messageColors;
        private readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(3);

        public MessageService(Panel messagePanel)
        {
            _messagePanel = messagePanel ?? throw new ArgumentNullException(nameof(messagePanel));
            _messageColors = new Dictionary<MessageType, Brush>
            {
                { MessageType.Info, Brushes.Black },
                { MessageType.Success, Brushes.DarkOliveGreen },
                { MessageType.Warning, Brushes.DarkGoldenrod },
                { MessageType.Error, Brushes.DarkRed }
            };
        }
        public void ShowMessage(string message, MessageType messageType, bool useTimer = true, TimeSpan? duration = null)
        {
            if (_messagePanel == null) return;
            if (!_messagePanel.Dispatcher.CheckAccess())
            {
                _messagePanel.Dispatcher.Invoke(() => ShowMessage(message, messageType, useTimer, duration));
                return;
            }
            TextBlock tb = new TextBlock
            {
                Text = message ?? string.Empty,
                Foreground = _messageColors.ContainsKey(messageType) ? _messageColors[messageType] : Brushes.Black,
                Margin = new System.Windows.Thickness(2, 2, 2, 2),
                FontSize = 25,
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            _messagePanel.Children.Add(tb);

            if (!useTimer) return;

            TimeSpan interval = duration ?? _defaultDuration;
            DispatcherTimer timer = new DispatcherTimer { Interval = interval };
            timer.Tick += (s, e) =>
            {
                try
                {
                    timer.Stop();
                    if (_messagePanel.Children.Contains(tb))
                    {
                        DoubleAnimation fadeOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = TimeSpan.FromSeconds(1)
                        };

                        fadeOut.Completed += (s2, e2) =>
                        {
                            if (_messagePanel.Children.Contains(tb))
                                _messagePanel.Children.Remove(tb);
                        };

                        tb.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
                    }
                }
                catch
                {
                }
                finally
                {
                    timer.Tick -= null;
                }
            };
            timer.Start();
        }
    }
}

