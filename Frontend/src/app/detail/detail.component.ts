import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Caff } from '../models/caff';
import { CComment } from '../models/comment';
import { CaffService } from '../services/caffservice';
import { CommentService } from '../services/commentservice';


@Component({
  selector: 'app-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class DetailComponent {
  caff?: Caff;
  commentFormControl = new FormControl('');
  cid: string="";

  constructor(private route: ActivatedRoute, private caffService: CaffService, private commentService: CommentService) { }

  ngOnInit(): void {
    this.cid = this.route.snapshot.paramMap.get('id') as string;
    this.getCaffDetail(this.cid);

  }
  getCaffDetail(id: string) {
    this.caffService.getCaffById(id).subscribe(res => {
      this.caff = res;
    })
  }

  sendComment() {
    let c = { id: this.cid, text: this.commentFormControl.value!, uploaderid: 1, username: "" };
    this.commentService.createComment(c).subscribe(res => {

    })
  }
}
