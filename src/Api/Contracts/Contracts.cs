namespace InterviewHub.Api.Contracts;

using InterviewHub.Domain;

public record CreateQuestionRequest(string Text, string Answer, Difficulty Difficulty, Guid CategoryId);

public record QuestionResponse(
    Guid Id, string Text, string Answer, Difficulty Difficulty,
    bool IsReviewed, Guid CategoryId, string CategoryName);

public record CreateCategoryRequest(string Name);

public record CategoryResponse(Guid Id, string Name);