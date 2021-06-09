namespace UnitOfWorkContextCore.Interfaces
{
    public interface IUnitOfWorkspace
    {
        IRepository<T> GetRepository<T>() where T : class;

        void OpenTransaction();

        void Commit();
    }
}
