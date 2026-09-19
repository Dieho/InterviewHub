// app.routes.ts
import { Routes } from '@angular/router';
import { QuestionsListComponent } from './features/questions-list/questions-list';
import { LoginComponent } from './features/login/login';
import { QuestionFormComponent } from './features/question-form/question-form';
import { CategoryFormComponent } from './features/category-form/category-form';
import { authGuard } from './core/guards/auth-guard';
import { adminGuard } from './core/guards/admin-guard';

export const routes: Routes = [
  { path: '', component: QuestionsListComponent },
  { path: 'login', component: LoginComponent },
  { path: 'add-question', component: QuestionFormComponent, canActivate: [authGuard]},
  { path: 'add-category', component: CategoryFormComponent, canActivate: [authGuard, adminGuard] },
];