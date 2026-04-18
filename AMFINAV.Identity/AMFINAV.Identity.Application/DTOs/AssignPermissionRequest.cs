namespace AMFINAV.Identity.Application.DTOs;

public class AssignPermissionRequest
{
    public string Resource { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new(); // Read, Create, Update, Delete, Approve
}