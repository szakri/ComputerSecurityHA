import { HttpClient, HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, pipe, tap } from "rxjs";
import * as moment from 'moment';

import { User } from "../models/user";
import { environments } from "../../environments/environment";
import { LoginResponse } from "../models/loginresponse";
import { Router } from "@angular/router";

const httpOptions = {
  headers: new HttpHeaders({ 'Content-Type': 'application/json;' })
};

@Injectable()
export class AuthService {
  backendUrl = environments.backendUrl;
  constructor(private http: HttpClient, private router: Router) {}
  login(username: string | null, password: string | null) {


    return this.http.get<LoginResponse>(this.backendUrl + '/test/login?username=' + username + "&password=" + password, httpOptions)
      .pipe(tap(res => {
        const expiresAt = moment().add(7200, 'second');

        localStorage.setItem('id_token', JSON.stringify(res.token));
        localStorage.setItem("expires_at", JSON.stringify(expiresAt.valueOf()));
        localStorage.setItem('user_id', JSON.stringify(res.userId));
        localStorage.setItem('user_name', JSON.stringify(username));

      })).subscribe(r => {
        console.log("token: " + r);
        this.router.navigate(['main']);
      });
  }

        register(username: string | null, password: string | null) {
          return this.http.post<User>(this.backendUrl + '/users', { username, password })
                  .subscribe(res => this.login(username, password));
        }

        logout() {
          localStorage.removeItem("id_token");
          localStorage.removeItem("expires_at");
          localStorage.removeItem("user_id");
          localStorage.removeItem("user_name");
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
