using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Core.ApiClients.Abstracts;
using TaskManager.Core.ApiClients.Dtos;
using TaskManager.Models.Models;

namespace TaskManager.Core.ApiClients;

public class JsonPlaceholderTaskApiClient : ITaskApiClient
{
    private const string RequestUri = "todos";
    private const string EmptyResponseMessage = "Server returned an empty response.";
    private const string InvalidResponseMessage = "Server returned tasks in an unexpected format.";
    private const string NetworkErrorMessage = "Unable to load tasks. Check your internet connection and try again.";
    private const string TimeoutMessage = "Task loading timed out. Try again later.";
    private const string UnexpectedErrorMessage = "Unexpected error occurred while loading tasks.";
    
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
                return OperationResult<IReadOnlyList<TaskItem>>.Fail(
                    $"Server returned error {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            var todos = await response.Content
                .ReadFromJsonAsync<List<TodoDto>>(cancellationToken)
                .ConfigureAwait(false);

            if (todos is null)
            {
                return OperationResult<IReadOnlyList<TaskItem>>.Fail(EmptyResponseMessage);
            }

            var tasks = todos.Select(x => new TaskItem
            {
                Id = x.Id,
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
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(TimeoutMessage);
        }
        catch (HttpRequestException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(NetworkErrorMessage);
        }
        catch (JsonException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(InvalidResponseMessage);
        }
        catch (NotSupportedException)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(InvalidResponseMessage);
        }
        catch (Exception)
        {
            return OperationResult<IReadOnlyList<TaskItem>>.Fail(UnexpectedErrorMessage);
        }
    }
}
