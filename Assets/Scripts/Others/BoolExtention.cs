using UnityEngine;

namespace Extention
{
    public static class BoolExtention
    {
        /// <summary>�^�U�l�𔽓]������</summary>
        /// <returns>true -> false | false -> true</returns>
        public static bool Invert(this bool isFrag) => !isFrag;
    }
}
