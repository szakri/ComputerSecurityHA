import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environments } from "../../environments/environment";
import { CComment } from "../models/comment";
import { CommentPost } from "../models/commentpost";

@Injectable({
  providedIn: 'root',
})
export class CommentService {

  backendUrl = environments.backendUrl;
  constructor(private http: HttpClient) { }

  createComment(newComment: CommentPost) {
    return this.http.post<CComment>(this.backendUrl + '/comments', newComment);
  }

}
