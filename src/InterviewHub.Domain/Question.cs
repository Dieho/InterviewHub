namespace InterviewHub.Domain;

    public enum Difficulty { Junior, Middle, Senior }

    public class Question
    {
        public Guid Id { get; private set; }
        public string Text { get; private set; } = null!;
        public string Answer { get; private set; } = null!;
        public Difficulty Difficulty { get; private set; }
        public bool IsReviewed { get; private set; }

        public Guid CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;

        private Question()
        {
        } // для EF Core (потребує parameterless конструктор)

        public Question(string text, string answer, Difficulty difficulty, Category category)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Текст питання не може бути пустим", nameof(text));

            Id = Guid.NewGuid();
            Text = text;
            Answer = answer;
            Difficulty = difficulty;
            Category = category;
            CategoryId = category.Id;
        }

        // Поведінка в самій Entity, а не в сервісі — Aggregate Root підхід з DDD-блоку
        public void MarkAsReviewed() => IsReviewed = true;

        public void UpdateAnswer(string newAnswer)
        {
            if (string.IsNullOrWhiteSpace(newAnswer))
                throw new ArgumentException("Відповідь не може бути пустою", nameof(newAnswer));
            Answer = newAnswer;
            IsReviewed = false; // логічне правило: зміна відповіді скидає "переглянуто"
        }
    }