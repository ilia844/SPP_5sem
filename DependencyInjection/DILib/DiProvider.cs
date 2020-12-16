namespace DILib
{
    public class DiProvider
    {
        private readonly DiConfig _diConfig;

        public DiProvider(DiConfig diConfig)
        {
            _diConfig = diConfig;
        }

        public T Inject<T>()
        {
            return (T) _diConfig.Get(typeof(T));
        }
    }
}