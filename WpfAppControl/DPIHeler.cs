using Microsoft.Win32;
using System;

namespace WpfAppControl
{
    /// <summary>
    /// </summary>
    public static class DPIHeler
    {
        private static double _dpiPix = double.NaN;

        /// <summary>
        /// </summary>
        public static double DpiPix
        {
            get
            {
                if (double.NaN.Equals(_dpiPix))
                {
                    _dpiPix = GetDPIPiex();
                }
                return _dpiPix;
            }
        }

        private static double _percentage = double.NaN;

        /// <summary>
        /// </summary>
        public static double Percentage
        {
            get
            {
                if (double.NaN.Equals(_percentage))
                {
                    switch (DpiPix)
                    {
                        case 96D:
                            _percentage = 1.0D;
                            break;

                        case 120D:
                            _percentage = 1.25D;
                            break;

                        case 144D:
                            _percentage = 1.5D;
                            break;

                        case 192D:
                            _percentage = 2.0D;
                            break;

                        default:
                            _percentage = DpiPix / 96D;
                            break;
                    }
                }
                return _percentage;
            }
        }

        private static double GetDPIPiex()
        {
            var returnValue = 96D;
            try
            {
                using (RegistryKey key = Registry.CurrentUser)
                {
                    using (RegistryKey pixeKey2 = key.OpenSubKey("Control Panel\\Desktop\\WindowMetrics"))
                    {
                        if (pixeKey2 != null)
                        {
                            var pixels = pixeKey2.GetValue("AppliedDPI");
                            if (pixels != null)
                            {
                                returnValue = double.Parse(pixels.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Can not found DPI value:\t" + ex.Message);
                returnValue = 96D;
            }
            return returnValue;
        }
    }
}