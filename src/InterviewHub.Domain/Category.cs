namespace InterviewHub.Domain;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    private readonly List<Question> _questions = new();
    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

    private Category() { } // для EF Core

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва категорії не може бути пустою", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
    }
}