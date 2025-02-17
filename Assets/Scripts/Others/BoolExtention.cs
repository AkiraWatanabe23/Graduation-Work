using UnityEngine;

namespace Extention
{
    public static class BoolExtention
    {
        /// <summary>真偽値を反転させる</summary>
        /// <returns>true -> false | false -> true</returns>
        public static bool Invert(this bool isFrag) => !isFrag;
    }
}
