using System.Net;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}