import { Component } from '@angular/core';
import { AuthService } from '../services/authservice';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {

  constructor(private authService: AuthService) { }

  logoutFromWebshop() {
    this.authService.logout();
  }

  isloggedin() {
    return this.authService.isLoggedIn();
  }
}
