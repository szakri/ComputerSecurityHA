import { Injectable } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Observable, throwError } from "rxjs";

@Injectable({
  providedIn: 'root',
})
export class ErrorService {

  constructor(private snackbar: MatSnackBar) { }

  catchServiceError(error: any): Observable<Response> {
    if (error && error.error) {
      this.snackbar.open(error.error, "", { duration: 3000 });
    } else if (error) {
      this.snackbar.open(error, "", { duration: 3000 });
    } else {
      this.snackbar.open(JSON.stringify(error), "", { duration: 3000 });
    }
    return throwError(() => new Error(error));
  }
}
