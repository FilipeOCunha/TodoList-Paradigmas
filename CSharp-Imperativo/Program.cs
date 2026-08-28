var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("TodoPolicy", policy =>
    {
        policy
            .WithOrigins("https://vmussak.github.io")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("TodoPolicy");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Lista em memória
var todos = new List<Todo>
{
    new Todo(1, "Conhecer o contrato da API", true),
    new Todo(2, "Implementar o primeiro paradigma", false)
};

// GET /todos
app.MapGet("/todos", () =>
{
    return Results.Ok(todos);
});

// POST /todos
app.MapPost("/todos", (CreateTodoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new
        {
            error = "O título da tarefa é obrigatório."
        });
    }

    int nextId = 1;

    foreach (var todo in todos)
    {
        if (todo.Id >= nextId)
        {
            nextId = todo.Id + 1;
        }
    }

    var newTodo = new Todo(
        nextId,
        request.Title.Trim(),
        false
    );

    todos.Add(newTodo);

    return Results.Created($"/todos/{newTodo.Id}", newTodo);
});

// PATCH /todos/{id}/toggle
app.MapPatch("/todos/{id:int}/toggle", (int id) =>
{
    Todo? todoEncontrado = null;

    foreach (var item in todos)
    {
        if (item.Id == id)
        {
            todoEncontrado = item;
            break;
        }
    }

    if (todoEncontrado is null)
    {
        return Results.NotFound(new
        {
            error = "Tarefa não encontrada."
        });
    }

    todoEncontrado.Completed = !todoEncontrado.Completed;

    return Results.Ok(todoEncontrado);
});

// DELETE /todos/{id}
app.MapDelete("/todos/{id:int}", (int id) =>
{
    Todo? todoEncontrado = null;

    foreach (var item in todos)
    {
        if (item.Id == id)
        {
            todoEncontrado = item;
            break;
        }
    }

    if (todoEncontrado is null)
    {
        return Results.NotFound(new
        {
            error = "Tarefa não encontrada."
        });
    }

    todos.Remove(todoEncontrado);

    return Results.NoContent();
});

// GET /health
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "ok"
    });
});

app.Run();

class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool Completed { get; set; }

    public Todo(int id, string title, bool completed)
    {
        Id = id;
        Title = title;
        Completed = completed;
    }
}

record CreateTodoRequest(string Title);