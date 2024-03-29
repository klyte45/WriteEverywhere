﻿namespace WriteEverywhere.Rendering
{
    public class FormattableString
    {
        private string value;
        private string upper;
        private string lower;
        private string abbreviated;
        private string lowerAbbreviated;
        private string upperAbbreviated;

        public FormattableString(string value) => Value = value;

        public string Get(bool uppercase, bool abbreviated)
            => uppercase
                ? abbreviated ? UpperAbbreviated : Upper
                : abbreviated ? Abbreviated : Value;

        public string GetFormatted(string format)
            => format is null
                ? Value
                : format.Contains("U")
                    ? format.Contains("A") ? UpperAbbreviated : Upper
                    : format.Contains("L")
                        ? format.Contains("A") ? LowerAbbreviated : Lower
                        : format.Contains("A") ? Abbreviated : Value;

        public string Value
        {
            get => value; set
            {
                this.value = value ?? "";
                upper = null;
                lower = null;
                abbreviated = null;
                lowerAbbreviated = null;
                upperAbbreviated = null;
            }
        }
        public string Upper
        {
            get
            {
                if (upper is null)
                {
                    upper = value.ToUpper();
                }
                return upper;
            }
        }
        public string Abbreviated
        {
            get
            {
                if (abbreviated is null)
                {
                    abbreviated = value;//TODO: Use ADR bridge!!!
                }
                return abbreviated;
            }
        }
        public string UpperAbbreviated
        {
            get
            {
                if (upperAbbreviated is null)
                {
                    upperAbbreviated = Abbreviated.ToUpper();
                }
                return upperAbbreviated;
            }
        }

        public string LowerAbbreviated
        {
            get
            {
                if (lowerAbbreviated is null)
                {
                    lowerAbbreviated = Abbreviated.ToLower();
                }
                return lowerAbbreviated;
            }
        }
        public string Lower
        {
            get
            {
                if (lower is null)
                {
                    lower = Value.ToLower();
                }
                return lower;
            }
        }

    }

}
