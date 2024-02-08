import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CitiesComponent } from './cities/cities.component';
import { SignupUpPageComponent } from './Components/signup-up-page/signup-up-page.component';
import { LoginPageComponent } from './Components/login-page/login-page.component';

const routes: Routes = [
  { path: "cities", component: CitiesComponent },
  {path:"login", component:LoginPageComponent},
  {path:"signUp", component:SignupUpPageComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
