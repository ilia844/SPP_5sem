namespace DILib
{
    public class Single : IGenerator
    {
        private readonly Create _create;
        private object _instance;

        public object Generate()
        {
            return _instance ??= _create();
        }

        public Single(Create create)
        {
            _create = create;
        }
    }
}