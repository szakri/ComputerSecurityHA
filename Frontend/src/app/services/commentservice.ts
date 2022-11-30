import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CComment } from "../models/comment";

@Injectable({
  providedIn: 'root',
})
export class CommentService {

  private baseUrl = 'api/comments';
  constructor(private http: HttpClient) { }

  getComments(): Observable<CComment[]> {
    return this.http.get<CComment[]>(this.baseUrl);
  }

  createComment(newComment: CComment) {
    return this.http.post<CComment>(this.baseUrl, newComment );
  }

}
