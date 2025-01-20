namespace Weaviate.Client;

public class ClassificationsApi
{
    private readonly Transport _transport;

    public ClassificationsApi(Transport transport)
    {
        _transport = transport;
    }

    public ApiResponse<ClassificationResponse> Schedule(ScheduleClassificationRequest request)
    {
        return _transport.Post<ClassificationResponse, ScheduleClassificationRequest>("/classifications", request);
    }

    public ApiResponse<ClassificationResponse> Get(GetClassificationRequest request)
    {
        return _transport.Get<ClassificationResponse>($"/classifications/{request.Id}");
    }
}
