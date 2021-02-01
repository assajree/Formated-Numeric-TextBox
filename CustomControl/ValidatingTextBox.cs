using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;

namespace CustomControl
{

    public class DecimalTextBox : TextBox
    {
        decimal _Value = 0;
        int _CommaCount = 0;
        int _CursorPosition = -1;
        bool _FinishChange = true;
        string _Text;

        string _FormatString = "{0:#,####0.0000}";
        public string FormatString
        {
            get
            {
                return _FormatString;
            }
            set
            {
                _FormatString = value;
                this.Text = string.Format(_FormatString, 0);
            }
        }

        public DecimalTextBox()
        {
            this.Text = string.Format(FormatString, 0);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            try
            {
                if (_FinishChange)
                {
                    _CursorPosition = this.SelectionStart;

                    // fix cursor position when type first value after 0
                    if (_Value == 0 && _CursorPosition == 2)
                        _CursorPosition -= 1;
                }

                _FinishChange = false;
              

                decimal val = decimal.Parse(this.Text.Replace(",", "")); 
                string text = string.Format(FormatString, val);
                int commaCount = GetCharCount(',', text);
                
                if (commaCount > _CommaCount)
                {
                    _CursorPosition += 1;
                }
                else if (commaCount < _CommaCount && _CursorPosition > 0)
                {
                    // move positoion if comma deleted
                    // do only if delete single character
                    if (_Text.Replace(",", "").Length - text.Replace(",", "").Length == 1)
                    {
                        _CursorPosition -= 1;
                    }
                }

                _CommaCount = commaCount;

                this.Text = text;

                if (_CursorPosition != -1)
                    this.SelectionStart = _CursorPosition;

                _CursorPosition = -1;
                _FinishChange = true;

            }
            catch
            {
                if(!String.IsNullOrEmpty(this.Text))
                    this.Text = string.Format(FormatString, 0);
            }
        }

        private int GetCharCount(char c)
        {
            return this.Text.Count(f => f == c);
        }

        private int GetCharCount(char c, string str)
        {
            return str.Count(f => f == c);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // prevent delete of dot by delete button
            if (e.KeyCode == Keys.Delete && this.SelectionStart == this.Text.IndexOf('.'))
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            _Text = this.Text;

            // input first digit before 0
            Decimal.TryParse(this.Text, out _Value);
            if (_Value == 0 && this.SelectionStart == 0)
            {
                this.Text = String.Empty;
                _CursorPosition = 1;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.'))
            {
                if (!_FormatString.Contains('.'))
                {
                    e.Handled = true;
                }
                else if ((this.Text.IndexOf('.') > -1))
                {
                    e.Handled = true;

                    //// move next when press .
                    //int commaPosition = this.Text.IndexOf('.');
                    //if (this.SelectionStart == commaPosition)
                    //    this.SelectionStart += 1;

                    // move to dot
                    int commaPosition = this.Text.IndexOf('.');
                    this.SelectionStart = commaPosition + 1;
                }
                
            }

            // prevent delete of dot by backspace
            if (e.KeyChar == (char)Keys.Back && this.SelectionStart == this.Text.IndexOf('.') + 1)
            {
                e.Handled = true;

                if(this.SelectionStart >0)
                    this.SelectionStart -= 1;
            }

        }
    }

}
