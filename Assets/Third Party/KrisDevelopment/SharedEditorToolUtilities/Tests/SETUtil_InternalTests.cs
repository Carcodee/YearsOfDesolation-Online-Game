#if UNITY_INCLUDE_TESTS && ENABLE_SETUTIL_TESTS
using NUnit.Framework;

namespace SETUtil.Internal.Tests
{
    public class SETUtil_InternalTests
    {
        [Test]
        public void InternalInvoker_Test()
        {
#if UNITY_HDRP
            Assert.IsTrue(SETUtil.Volatile.InternalInvokerUtils.GetHDRPToolMethodInfo() != null, "Couldn't find HDRP method");
#endif

#if UNITY_URP
            Assert.IsTrue(SETUtil.Volatile.InternalInvokerUtils.GetURPToolMethodInfo() != null, "Couldn't find URP method");
#endif
        }
    }
}
#endif