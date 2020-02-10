namespace CryShader.Shaders
{
    public partial class G
    {
        internal static T VERIFY<T>(Func<T> func)
        {
            return func();
        }
    }

    public class CRenderer
    {
        public CShaderMan m_cEF;
    }

    public interface ILog
    {
        void Log(string format, params object[] args);
    }

    public static class Core
    {
        public static ILog iLog;
        public static CRenderer gRenDev;
    }
}
