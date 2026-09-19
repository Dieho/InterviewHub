// features/questions-list/questions-list.ts
import { Component, OnInit, signal } from '@angular/core';
import { QuestionService, Question } from '../../core/services/question';

@Component({
  selector: 'app-questions-list',
  standalone: true,
  imports: [],
  templateUrl: './questions-list.html',
  styleUrl: './questions-list.css'
})
export class QuestionsListComponent implements OnInit {
  questions = signal<Question[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(private questionService: QuestionService) {}

  ngOnInit(): void {
    this.questionService.getAll().subscribe({
      next: (data) => {
        this.questions.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load questions', err);
        this.error.set('Не вдалося завантажити питання');
        this.loading.set(false);
      }
    });
  }
}