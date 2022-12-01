import { HttpClient, HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, pipe, tap } from "rxjs";
import * as moment from 'moment';

import { User } from "../models/user";
import { environments } from "../../environments/environment";

const httpOptions = {
  headers: new HttpHeaders({ 'Content-Type': 'application/json' })
};

@Injectable()
export class AuthService {
  backendUrl = environments.backendUrl;
   constructor(private http: HttpClient) {}
  login(username: string | null, password: string | null) {


    return this.http.get(this.backendUrl + '/test/login?username=' + username + "&password=" + password, httpOptions)
      .pipe(tap(res => {
        const expiresAt = moment().add(7200, 'second');

        localStorage.setItem('id_token', JSON.stringify(res));
        localStorage.setItem("expires_at", JSON.stringify(expiresAt.valueOf()));

      })).subscribe(r => console.log("token: " + r));
  }

  register(username: string | null, password: string | null) {
    return this.http.post<User>(this.backendUrl + '/users', { username, password })
            .subscribe(res => this.setSession);
  }

  private setSession(authResult: { expiresIn: any; idToken: string; }) {
    console.log("setsession");
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
