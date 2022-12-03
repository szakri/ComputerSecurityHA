import { Component, Inject, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog, MatDialogConfig, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { Caff } from '../models/caff';
import { CommentPost } from '../models/commentpost';
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
  cid: string = "";
  isAdmin = false;
  imageToShow: any;

  constructor(private route: ActivatedRoute, private caffService: CaffService, private commentService: CommentService, public dialog: MatDialog, private router: Router) {
    let userName = localStorage.getItem('user_name');
    this.isAdmin = userName === "\"Admin\"";
  }

  ngOnInit(): void {
    this.cid = this.route.snapshot.paramMap.get('id') as string;
    this.getCaffDetail(this.cid);
    
  }
  getCaffDetail(id: string) {
    this.caffService.getCaffById(id).subscribe(res => {
      this.caff = res;
      this.caffService.getCaffPreview(id).subscribe(res1 => {
        const reader = new FileReader();
        reader.addEventListener("load", () => {
          this.imageToShow = reader.result;
        }, false);
        if (res1) {
          reader.readAsDataURL(res1);
        }
      });
    })
  }

  sendComment() {
    let userId = localStorage.getItem('user_id')?.replaceAll("\"", "");
    let c: CommentPost = { caffId: this.caff.id, userId: userId!, commentText: this.commentFormControl.value! };
    this.commentService.createComment(c).subscribe(res => {
      this.getCaffDetail(this.caff.id);
      this.commentFormControl.setValue("");
    })
  }

  downloadCaff() {
    this.caffService.downloadCaff(this.caff.id).subscribe(res => {
      let downloadURL = URL.createObjectURL(res);
      const link = document.createElement('a');
      link.setAttribute('target', '_blank');
      link.setAttribute('href', downloadURL);
      link.setAttribute('download', this.caff.name+'.caff');
      document.body.appendChild(link);
      link.click();
      link.remove();
    });
  }

  deleteCAFF() {
    const dialogRef = this.dialog.open(DialogConfirmDelete);
    dialogRef.afterClosed().subscribe(result => {
      if (result == "yes") {
        this.caffService.deleteCaff(this.caff.id).subscribe(res =>
          this.router.navigate(['main'])
        );
      }
    });
  }

  openEditDialog() {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.data = {
      name: this.caff.name
    };
    const dialogRef = this.dialog.open(DialogEditCAFF, dialogConfig);
    dialogRef.afterClosed().subscribe(result => {
      if (result != null) {
        this.caffService.modifyCaff(result, this.caff.id).subscribe(res =>
          this.getCaffDetail(this.caff.id)
        );
      }
    });
  }

}

@Component({
  selector: 'dialog-data-example-dialog',
  templateUrl: 'dialog-edit.html',
})
export class DialogEditCAFF implements OnInit {
  nameFormControl = new FormControl('');

  constructor(public dialogRef: MatDialogRef<DialogEditCAFF>, @Inject(MAT_DIALOG_DATA) public data: Caff) { }

  ngOnInit() {
    this.nameFormControl.setValue(this.data.name);
  }

  cancelDialog() {
    this.dialogRef.close();
  }

  saveDialog() {
    this.dialogRef.close(this.nameFormControl.value);
  }
}

@Component({
  selector: 'dialog-data-example-dialog',
  templateUrl: 'dialog-confirm-delete.html',
})
export class DialogConfirmDelete {

  constructor(public dialogRef: MatDialogRef<DialogConfirmDelete>) { }


  cancelDialog() {
    this.dialogRef.close();
  }

  confirmDialog() {
    this.dialogRef.close("yes");
  }
}
