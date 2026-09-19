// features/question-form/question-form.ts
import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { QuestionService } from '../../core/services/question';
import { CategoryService, Category } from '../../core/services/category';

@Component({
  selector: 'app-question-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './question-form.html',
  styleUrl: './question-form.scss'
})
export class QuestionFormComponent implements OnInit {
  form: FormGroup;
  categories = signal<Category[]>([]);
  errorMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  constructor(
    private fb: FormBuilder,
    private questionService: QuestionService,
    private categoryService: CategoryService,
    private router: Router
  ) {
    this.form = this.fb.group({
      text: ['', Validators.required],
      answer: ['', Validators.required],
      difficulty: ['Middle', Validators.required],
      categoryId: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories.set(data),
      error: (err) => console.error('Failed to load categories', err)
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.questionService.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(
          err.status === 401 ? 'Потрібно увійти' : 'Не вдалося створити питання'
        );
      }
    });
  }
}