using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PR1_0101
{
    public class ConfirmCloseWindow : Window
    {
        private bool _isClosingProgrammatically = false;

        public new void Close()
        {
            _isClosingProgrammatically = true;
            base.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_isClosingProgrammatically)
            {
                _isClosingProgrammatically = false; 
                base.OnClosing(e);
                return;
            }

            var result = MessageBox.Show(
                "Вы уверены, что хотите закрыть это окно?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                base.OnClosing(e);
            }
        }
    }
}
