using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCRHQ_to_Magic.Datasources
{
    class TableSettings
    {
        public string table { get; set; }
        public string record_type { get; set; }
        public string export_file { get; set; }
        public string stored_procedure { get; set; }
        public bool use_stored_proc { get; set; }
        public string sql_filename { get; set; }
        public bool use_store_number { get; set; }
        public bool active { get; set; }
    }
}
