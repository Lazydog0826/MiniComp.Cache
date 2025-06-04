using Grpc.Core;
using MiniComp.Core.App;
using MiniComp.Core.App.CoreException;

namespace MiniComp.Cache;

public class LockTimeoutException : CustomException
{
    public LockTimeoutException()
        : base(new Status(StatusCode.DeadlineExceeded, "操作超时"), "操作超时") { }

    public override WebApiResponse GetWebApiCallBack()
    {
        return WebApiResponse.Error("操作超时");
    }
}
