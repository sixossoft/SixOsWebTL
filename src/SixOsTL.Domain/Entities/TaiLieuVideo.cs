using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixOsTL.Domain.Entities
{
    public class TaiLieuVideo
    {
        public long Id { get; set; }
        public string? STT { get; set; }
        public long IDChucNang { get; set; }
        public string? TenVideo { get; set; }
        public string? Keyword { get; set; }
        public string? DuongDanFileVideo { get; set; }
        public bool Active { get; set; }

        public DmChucNang ChucNang { get; set; } = default!;
        public ICollection<TaiLieuVideoTagMap> VideoTagMaps { get; set; } = new List<TaiLieuVideoTagMap>();
        public ICollection<TaiLieuVideoLienQuan> LienQuans { get; set; } = new List<TaiLieuVideoLienQuan>();
        public ICollection<LichSuXemVideo> LichSuXemVideos { get; set; } = new List<LichSuXemVideo>();
    }
}
