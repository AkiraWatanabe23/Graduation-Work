using Network;
using UnityEngine;

public class RequestButton : MonoBehaviour
{
    [SerializeField]
    private RequestType _requestType = RequestType.None;

    public RequestType RequestType => _requestType;
}
