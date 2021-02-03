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
        int _DecimalDigit = 4;

        string _FormatString = "#,####0.0000";
        public string FormatString
        {
            get
            {
                return _FormatString;
            }
            set
            {
                _FormatString = value;
                _DecimalDigit = GetDecimalDigit(value);

                RefreshValue();

            }
        }

        private void RefreshValue()
        {
            decimal val = decimal.Parse(this.Text.Replace(",", ""));
            this.Text = val.ToString(_FormatString);
        }

        private int GetDecimalDigit(string format)
        {
            string result = 0.ToString(format);
            int dotPosition = result.IndexOf('.');
            if (dotPosition == -1)
                return 0;

            string afterDot = result.Substring(dotPosition + 1, result.Length - (dotPosition + 1));
            return afterDot.Length;
        }

        public DecimalTextBox()
        {
            this.Text = 0.ToString(_FormatString);
        }

        public static decimal Floor(decimal aValue, int aDigits)
        {
            decimal m = (decimal)Math.Pow(10, aDigits);
            aValue *= m;
            aValue = (decimal)Math.Floor((double)aValue);
            return aValue / m;
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
            _Text = this.Text;

            // prevent delete of dot by delete button
            if (e.KeyCode == Keys.Delete && this.SelectionStart == this.Text.IndexOf('.'))
            {
                e.Handled = true;
            }

            // press delete before comma
            if (e.KeyCode == Keys.Delete && this.Text.Substring(this.SelectionStart, 1) == ",")
            {
                // remove comma and character after comma
                e.Handled = true;

                // for maintain cursor position after delete
                _FinishChange = false;
                _CursorPosition = this.SelectionStart;
                _CommaCount -=1;

                string newText = this.Text.Substring(0, this.SelectionStart) + this.Text.Substring(this.SelectionStart + 2, this.Text.Length - (this.SelectionStart + 2));
                this.Text = newText;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // input first digit before 0
            _Value = TryParseDecimal(this.Text);
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

                if (this.SelectionStart > 0)
                    this.SelectionStart -= 1;
            }

            base.OnKeyPress(e);

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
                string text = Floor(val, _DecimalDigit).ToString(_FormatString);
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

                base.OnTextChanged(e);

            }
            catch (Exception ex)
            {
                if (!String.IsNullOrEmpty(this.Text))
                    this.Text = 0.ToString(_FormatString);
            }
        }

        private decimal TryParseDecimal(string str)
        {
            try
            {
                return Decimal.Parse(str);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }

}
