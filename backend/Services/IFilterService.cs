using WebXuLyAnh.Api.Models;

namespace WebXuLyAnh.Api.Services;

public interface IFilterService
{
    FilterResponse ApplyFilter(MatrixFilterRequest request, List<WarningDto>? warnings = null);
    BatchFilterResponse ApplyBatchFilter(BatchFilterRequest request, List<WarningDto>? warnings = null);
}
