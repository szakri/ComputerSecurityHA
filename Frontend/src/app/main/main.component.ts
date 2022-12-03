import { Component, Inject, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Caff } from '../models/caff';
import { CaffPreview } from '../models/caffpreview';
import { CaffService } from '../services/caffservice';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent implements OnInit {
  searchFormControl = new FormControl('');
  caffs: Caff[] = [];
  caffPreviews: CaffPreview[] = [];
  gifsArrived = false;

  constructor(private caffService: CaffService, private dialog: MatDialog) { }

  filterCAFFs() {
    this.getCaffsByName(this.searchFormControl.value);
  }
  ngOnInit(): void {
    this.getCaffs();
  }
  getCaffs() {
    this.caffService.getCaffs(null).subscribe(res => {
      this.caffs = res;
      this.caffs.forEach(x => {
        let caffId = x.id;
        this.caffService.getCaffPreview(caffId).subscribe(res1 => {
          const reader = new FileReader();
          let imageToShow;
          if (res1) {
            reader.readAsDataURL(res1);
          }
          reader.addEventListener("load", () => {
            imageToShow = reader.result;
            let caffPrew = { id: caffId, gif: imageToShow };

            this.caffPreviews.push(caffPrew);
            if (this.caffPreviews.length == this.caffs.length) {
              this.gifsArrived = true;
            }
          }, false);

        });
      });
    })
  }

  getCaffsByName(searchterm: string | null) {
    if (searchterm != null) {
      this.caffService.getCaffs(searchterm).subscribe(res => {
        this.caffs = res;
      })
    }
  }

  getPreviewGif(id: string) {
    return this.caffPreviews.find(x => x.id == id)?.gif;
  }

  clearFormControl() {
    this.searchFormControl.setValue("");
    this.getCaffs();
  }

  openUploadDialog() {
    const dialogRef = this.dialog.open(DialogUploadCAFF);
    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      if (result != null) {
        this.getCaffs();
      }
    });
  }

}

@Component({
  selector: 'dialog-data-example-dialog',
  templateUrl: 'dialog-upload.html',
})
export class DialogUploadCAFF implements OnInit {
  fileName = '';
  formData = new FormData();
  file!: File;


  constructor(public dialogRef: MatDialogRef<DialogUploadCAFF>, private caffService: CaffService) { }

  ngOnInit() {
    
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    this.file = target.files![0];

    if (this.file) {

      this.fileName = this.file.name;

      this.formData.append("file", this.file);
      this.formData;
      
    }
  }

  cancelDialog() {
  }

  saveDialog() {
    const upload$ = this.caffService.uploadCaff(this.formData);

    upload$.subscribe(res => 
      this.dialogRef.close()
    );
  }
}
