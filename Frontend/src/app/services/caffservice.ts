import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable } from 'rxjs';
import { Caff } from '../models/caff';

@Injectable({
  providedIn: 'root',
})
export class CaffService {

  private baseUrl = 'https://localhost:7235/api/caffs';
  constructor(private http: HttpClient) { }

  getCaffs(searchby: string | null): Observable<Caff[]> {
    if (searchby) {
      return this.http.get<Caff[]>(this.baseUrl + "?name=" + searchby);
    }
    return this.http.get<Caff[]>(this.baseUrl);
  }


  getCaffById(id: string): Observable<Caff> {
    return this.http.get<Caff>(this.baseUrl + '/'+ id);
  }

  getCaffPreview(id: string): Observable<Caff> {
    return this.http.get<Caff>(this.baseUrl + '/' + id + '/preview');
  }

  //how does the file arrive?
  downloadCaff(id: string): Observable<Caff> {
    return this.http.get<Caff>(this.baseUrl + '/' + id + '/download');
  }
  //what is returning
  deleteCaff(id: string) {
    return this.http.delete<Caff>(this.baseUrl + '/' + id);
  }

  //valahogy fájlként kellene átpasszolni
  modifyCaff(newCaff: Caff, oldCaffId: string) {
    return this.http.post<Caff>(this.baseUrl, newCaff);
  }

}
