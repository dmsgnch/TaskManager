using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Core.ApiClients.Abstracts;
using TaskManager.Core.ApiClients.Dtos;
using TaskManager.Models.Constants;
using TaskManager.Models.Models;

namespace TaskManager.Core.ApiClients;

public class JsonPlaceholderTaskApiClient : ITaskApiClient
{
    private const string RequestUri = "todos";
    private readonly HttpClient _httpClient;

    public JsonPlaceholderTaskApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OperationResult<IReadOnlyList<TaskItem>>> GetTasksAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient
                .GetAsync(RequestUri, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksServerError);
            }

            var todos = await response.Content
                .ReadFromJsonAsync<List<TodoDto>>(cancellationToken)
                .ConfigureAwait(false);

            if (todos is null)
            {
                return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksEmptyResponse);
            }

            var tasks = todos.Select(x => new TaskItem(x.Id)
            {
                UserId = x.UserId,
                Title = x.Title,
                IsCompleted = x.Completed
            }).ToList();

            return OperationResult<IReadOnlyList<TaskItem>>.Success(tasks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksTimeout);
        }
        catch (HttpRequestException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksNetworkError);
        }
        catch (JsonException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksInvalidResponse);
        }
        catch (NotSupportedException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksInvalidResponse);
        }
        catch (Exception)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.LoadTasksUnexpectedError);
        }
    }
}
