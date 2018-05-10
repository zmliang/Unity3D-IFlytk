using UnityEngine;

namespace JinkeGroup.Util
{
    public static class BuildConfig
        {

            //有没有勾选Build里面的Development Build
            public static bool IsDevel
            {
                get
                {
                    return Debug.isDebugBuild;
                }
            }

            public static bool IsProd
            {
                get
                {
#if PROD_BUILD
                return true;
#else
                    return false;
#endif
                }
            }

            public static bool IsRelease
            {
                get
                {
                    return !IsProdOrDevel;
                }
            }

            public static bool IsProdOrDevel
            {
                get
                {
                    return IsDevel || IsProd;
                }
            }
        }
}
