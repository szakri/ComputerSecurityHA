import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable } from 'rxjs';
import { Caff } from '../models/caff';
import { environments } from '../../environments/environment';

const httpOptions = {
  headers: new HttpHeaders({ 'Content-Type': 'application/json;' })
};

@Injectable({
  providedIn: 'root',
})
export class CaffService {
  backendUrl = environments.backendUrl;
  constructor(private http: HttpClient) { }

  getCaffs(searchby: string | null): Observable<Caff[]> {
    if (searchby) {
      searchby = "Name==" +"\"" + searchby + "\"";
      return this.http.get<Caff[]>(this.backendUrl + "/caffs?searchBy="  + searchby );
    }
    else {
      return this.http.get<Caff[]>(this.backendUrl + '/caffs');
    }
  }


  getCaffById(id: string): Observable<Caff> {
    return this.http.get<Caff>(this.backendUrl + '/caffs/'+ id);
  }

  getCaffPreview(id: string) {
    return this.http.get<Caff>(this.backendUrl + '/caffs/' + id + '/preview');
  }

  uploadCaff(file: FormData) {
    const httpUploadOptions = {
      //headers: new HttpHeaders({ 'Content-Type': undefined })
    };
    return this.http.post(this.backendUrl + "/caffs?userId=" + localStorage.getItem('user_id')?.replaceAll("\"", ""), file);
  }

  downloadCaff(id: string){
    return `${this.backendUrl}/caffs/${id}/download`;
  }
  
  deleteCaff(id: string) {
    return this.http.delete(this.backendUrl + '/caffs/' + id);
  }

  
  modifyCaff(name: string, caffId: string) {
    return this.http.patch(this.backendUrl + "/caffs/" + caffId, `\"${name}\"`, httpOptions);
  }

}
