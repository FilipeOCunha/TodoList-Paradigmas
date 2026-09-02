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

var logger = app.Logger;

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
    logger.LogInformation(
        "[GET] Listando tarefas. Total: {Total}",
        todos.Count
    );

    return Results.Ok(todos);
});

// POST /todos
app.MapPost("/todos", (CreateTodoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        logger.LogWarning(
        "[POST] Tentativa de criar tarefa com título inválido."
    );

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

    logger.LogInformation(
        "[POST] Tarefa criada. ID: {Id} | Título: {Titulo}",
        newTodo.Id,
        newTodo.Title
    );

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
        logger.LogWarning(
            "[PATCH] Tarefa não encontrada. ID: {Id}",
            id
        );

        return Results.NotFound(new
        {
            error = "Tarefa não encontrada."
        });

    }

    todoEncontrado.Completed = !todoEncontrado.Completed;

    logger.LogInformation(
    "[PATCH] Tarefa alterada. ID: {Id} | Completed: {Completed}",
    todoEncontrado.Id,
    todoEncontrado.Completed
);

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
        logger.LogWarning(
            "[DELETE] Tarefa não encontrada. ID: {Id}",
            id
        );

        return Results.NotFound(new
        {
            error = "Tarefa não encontrada."
        });

    }

    todos.Remove(todoEncontrado);

    logger.LogInformation(
    "[DELETE] Tarefa removida. ID: {Id} | Título: {Titulo}",
    todoEncontrado.Id,
    todoEncontrado.Title
);

    return Results.NoContent();
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