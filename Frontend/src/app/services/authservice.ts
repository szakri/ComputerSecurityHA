import { HttpClient, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import * as moment from 'moment';

import { User } from "../models/user";



@Injectable()
export class AuthService {

        constructor(private http: HttpClient) {
        }

        login(email: string, password: string) {
          return this.http.post<User>('/api/login', { email, password })
            .subscribe(res => this.setSession);
        }

        private setSession(authResult: { expiresIn: any; idToken: string; }) {
          const expiresAt = moment().add(authResult.expiresIn, 'second');

          localStorage.setItem('id_token', authResult.idToken);
          localStorage.setItem("expires_at", JSON.stringify(expiresAt.valueOf()));
        }

        logout() {
          localStorage.removeItem("id_token");
          localStorage.removeItem("expires_at");
        }

        public isLoggedIn() {
          return moment().isBefore(this.getExpiration());
        }

        isLoggedOut() {
          return !this.isLoggedIn();
        }

        getExpiration() {
          const expiration = localStorage.getItem("expires_at");
          const expiresAt = JSON.parse(expiration!);
          return moment(expiresAt);
        }
}

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>,
    next: HttpHandler): Observable<HttpEvent<any>> {

    const idToken = localStorage.getItem("id_token");

    if (idToken) {
      const cloned = req.clone({
        headers: req.headers.set("Authorization",
          "Bearer " + idToken)
      });

      return next.handle(cloned);
    }
    else {
      return next.handle(req);
    }
  }
}
