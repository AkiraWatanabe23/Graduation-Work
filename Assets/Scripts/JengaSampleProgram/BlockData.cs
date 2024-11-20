using UnityEngine;

namespace Jenga
{
    public class BlockData : MonoBehaviour
    {
        public int BlockID { get; private set; } = 0;

        public void Init(int newID)
        {
            BlockID = newID;
        }
    }
}
