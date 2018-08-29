using System.Diagnostics;

namespace Dwragge.RCloneClient.Common
{
    public class DebugChecker
    {
        private static bool? _isDebugMode = null;

        public static bool IsDebug
        {
            get
            {
                if (_isDebugMode == null)
                {
                    _isDebugMode = false;
                    WellAreWe();
                }

                return _isDebugMode.Value;
            }
        }


        [Conditional("DEBUG")]
        private static void WellAreWe()
        {
            _isDebugMode = true;
        }
    }
}
