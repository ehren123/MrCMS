using MrCMS.Entities;
using NHibernate;

namespace MrCMS.Events
{
    public class OnAddingArgs
    {
        public SystemEntity Item { get; set; }
        public ISession Session { get; set; }
    }
}