using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static Trader.Lib.Enums;

namespace Trader.Lib
{
    public class MessageService
    {
        private readonly Panel _messagePanel;
        private readonly Dictionary<TextBlock, DispatcherTimer> _timers;
        private readonly Dictionary<MessageType, Brush> _messageColors;

        public MessageService(Panel messagePanel)
        {
            _messagePanel = messagePanel ?? throw new ArgumentNullException(nameof(messagePanel));
            _timers = new Dictionary<TextBlock, DispatcherTimer>();

            _messageColors = new Dictionary<MessageType, Brush>
        {
            { MessageType.Info, Brushes.Black },
            { MessageType.Success, Brushes.DarkOliveGreen },
            { MessageType.Warning, Brushes.DarkGoldenrod },
            { MessageType.Error, Brushes.DarkRed }
        };
        }
        public void ShowMessage(string message, MessageType messageType, bool useTimer = true)
        {
            if (_messagePanel == null)
                return;

            var allBlocks = _messagePanel.Children.OfType<TextBlock>().ToList();
            if (allBlocks.Count == 0)
                return;

            TextBlock emptyBlock = allBlocks.FirstOrDefault(tb => string.IsNullOrWhiteSpace(tb.Text));

            TextBlock target = emptyBlock
                               ?? allBlocks.FirstOrDefault(tb => tb.Text == message)
                               ?? allBlocks.First();
            Brush color = _messageColors.ContainsKey(messageType) ? _messageColors[messageType] : Brushes.White;

            if (_timers.TryGetValue(target, out DispatcherTimer existingTimer))
            {
                existingTimer.Stop();
                _timers.Remove(target);
            }

            target.Foreground = color;
            target.Text = message;

            if (useTimer)
            {
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, e) =>
                {
                    try
                    {
                        target.Text = string.Empty;
                    }
                    finally
                    {
                        timer.Stop();
                        _timers.Remove(target);
                    }
                };

                _timers[target] = timer;
                timer.Start();
            }
            
        }
    }
}
