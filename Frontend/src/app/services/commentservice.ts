import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environments } from "../../environments/environment";
import { CComment } from "../models/comment";
import { CommentPost } from "../models/commentpost";

@Injectable({
  providedIn: 'root',
})
export class CommentService {

  backendUrl = environments.backendUrl;
  constructor(private http: HttpClient) { }

  /*getComments(): Observable<CComment[]> {
    return this.http.get<CComment[]>(this.baseUrl);
  }*/

  createComment(newComment: CommentPost) {
    console.log('comment service comment: ' + newComment);
    return this.http.post<CComment>(this.backendUrl + '/comments', newComment);
  }

}
