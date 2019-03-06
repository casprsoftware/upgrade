using System.Collections.Generic;

namespace Upgrade
{
    public class Version
    {
        public int Id { get; set; }

        public IEnumerable<SqlFileInfo> Files { get; set; }

        public override string ToString()
        {
            return Id.ToString("000");
        }
    }

    public class SqlFileInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}