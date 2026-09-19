import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Question {
  id: string;
  text: string;
  answer: string;
  difficulty: 'Junior' | 'Middle' | 'Senior';
  isReviewed: boolean;
  categoryId: string;
  categoryName: string;
}

export interface CreateQuestionRequest {
  text: string;
  answer: string;
  difficulty: string;
  categoryId: string;
}

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private readonly apiUrl = 'http://localhost:5118/questions';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Question[]> {
    return this.http.get<Question[]>(this.apiUrl);
  }

  create(request: CreateQuestionRequest): Observable<Question> {
    return this.http.post<Question>(this.apiUrl, request);
  }
}