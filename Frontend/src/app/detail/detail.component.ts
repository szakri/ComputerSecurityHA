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
  caff!: Caff;
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

  getPreviewGif() {
    let url = "https://localhost:7235/api/caffs/" + this.caff.id + "/preview";
    return url;
  }

  downloadCaff() {
    const link = document.createElement('a');
    link.setAttribute('target', '_blank');
    link.setAttribute('href', this.caffService.downloadCaff(this.caff.id));
    link.setAttribute('download', 'selected.caff');
    document.body.appendChild(link);
    link.click();
    link.remove();
  }
}
