import { Component } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/authservice';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  userNameFormControl = new FormControl('', [Validators.required]);
  passwordFormControl = new FormControl('', [Validators.required]);

  constructor(private authService: AuthService) { }

  signUp() {
    this.authService.register(this.userNameFormControl.value, this.passwordFormControl.value);
  }
  signIn() {
    this.authService.login(this.userNameFormControl.value, this.passwordFormControl.value);
  };
 
}
